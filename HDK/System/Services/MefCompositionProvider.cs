using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Services
{
    public sealed class MefCompositionProvider : ICompositionProvider
    {
        private IServiceLocator m_ServiceLocator;
        private ContainerConfiguration _configuration;
        private CompositionHost _container;
        private ConventionBuilder _conventions;

        public MefCompositionProvider(IServiceLocator serviceLocator)
        {
            m_ServiceLocator = serviceLocator;
        }

        public event Func<ICompositionProvider, ContainerConfiguration, List<Assembly>, bool> PreCreateContainer;

        public event Action<ICompositionProvider, ContainerConfiguration> CreatingContainer;

        private ConventionBuilder Conventions
        {
            get { return _conventions ?? (_conventions = new ConventionBuilder()); }
        }

        public ContainerConfiguration Configuration
        {
            get
            {
                if (_configuration != null)
                    return _configuration;

                // Add conventions.
                Conventions.ForTypesDerivedFrom<IService>().Export<IService>().Shared(); //TODO BUG Dont work properly
                //Conventions.ForTypesDerivedFrom<IServiceLocator>().Export<IServiceLocator>().Shared();


                // Get a list of all the package assemblies.
                var assemblies = Task.Run(async () =>
                {
                    var localAssemblies = new List<Assembly>();
                    var result = await GetPackageAssemblyListAsync();
                    var theAsms = result.ToList();
                    foreach (var newAsm in theAsms.Where(asm => !localAssemblies.Contains(asm)
                        && !asm.FullName.StartsWith("System.Composition")))
                        localAssemblies.Add(newAsm);
                    return localAssemblies;
                }).Result;


                _configuration = new ContainerConfiguration();

                _configuration = _configuration.WithProvider(new ValueExportDescriptorProvider(m_ServiceLocator));

                //_configuration = _configuration.WithProvider(new InheritedExportDescriptorProvider());

                // Let any customization occur
                if (PreCreateContainer == null || !PreCreateContainer(this, _configuration, assemblies))
                    _configuration = _configuration.WithAssemblies(assemblies, Conventions);

                if (!assemblies.Contains(GetType().GetTypeInfo().Assembly))
                    _configuration = _configuration.WithAssembly(GetType().GetTypeInfo().Assembly, Conventions);

                return _configuration;
            }
        }

        public CompositionHost Container
        {
            get
            {
                if (CreatingContainer != null)
                    CreatingContainer(this, Configuration);
                return _container ?? (_container = Configuration.CreateContainer());
            }
        }

        public void Compose(object target)
        {
            try
            {
                Container.SatisfyImports(target);
            }
            catch (Exception exc)
            {
            }
        }

        public Lazy<T> GetInstance<T>() where T : class
        {
            // Special case the service locator if we've already used the built-in one.
            // This is done because the locator does not come through the MEF system itself 
            // and therefore isn't registered with the host.
            if ((typeof(T) == typeof(IServiceLocator) || typeof(T) == typeof(IServiceProvider)))
                return new Lazy<T>(() => (T)m_ServiceLocator);

            return new Lazy<T>(() => Container.GetExport<T>());
        }

        public Lazy<T> GetInstance<T>(string contractName)
        {
            return new Lazy<T>(() => Container.GetExport<T>(contractName));
        }

        public IEnumerable<T> GetInstances<T>()
        {
            return Container.GetExports<T>();
        }

        public Lazy<object> GetInstance(Type type)
        {
            return new Lazy<object>(() => Container.GetExport(type));
        }

        public Lazy<object> GetInstance(Type type, string contractName)
        {
            return new Lazy<object>(() => Container.GetExport(type, contractName));
        }

        public IEnumerable<object> GetInstances(Type type)
        {
            return Container.GetExports(type);
        }

        public IEnumerable<object> GetInstances(Type type, string contractName)
        {
            return Container.GetExports(type, contractName);
        }
        public IEnumerable<T> GetInstances<T>(string contractName)
        {
            return Container.GetExports<T>(contractName);
        }

        public ICompositionFactory<T> GetInstanceFactory<T>() where T : class
        {
            var factory = new MefCompositionFactory<T>();
            Container.SatisfyImports(factory);
            if (factory.ExportFactory == null)
                throw new CompositionFailedException();

            return factory;
        }

        /// <summary>
        /// This method retrieves all the assemblies in the current app package
        /// </summary>
        /// <returns></returns>
        internal async static Task<IEnumerable<Assembly>> GetPackageAssemblyListAsync()
        {
            var installFolder = global::Windows.ApplicationModel.Package.Current.InstalledLocation;

            // If we are in the designer, then load all subdirectories which have DLLs too.
            // This allows the ViewModelLocator and ServiceLocator to find elements properly
            // when being shadow copied
            if (Designer.InDesignMode)
            {
                // Grab all the possibilities
                var assemblies = new List<Assembly>();
                foreach (var folder in await installFolder.GetFoldersAsync())
                {
                    assemblies.AddRange((await folder.GetFilesAsync())
                            .Where(file => file.FileType == ".dll" || file.FileType == ".exe")
                            .Select(file => file.Name.Substring(0, file.Name.Length - file.FileType.Length))
                            .Distinct()
                            .Select(asmName => Assembly.Load(new AssemblyName(asmName))));
                }
                return assemblies;
            }

            // Otherwise we just look in the package folder.
            return ((await installFolder.GetFilesAsync())
                .Where(file => file.FileType == ".dll" || file.FileType == ".exe")
                .Select(file => file.Name.Substring(0, file.Name.Length - file.FileType.Length))
                .Select(asmName => Assembly.Load(new AssemblyName(asmName))))
                .ToList();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_container != null)
            {
                _container.Dispose();
                _container = null;
            }

            _container = null;
        }

        //private void Debug()
        //{
        //    var numberByName = new Dictionary<string, int>();
        //    var number = 1;

            

        //    Debug.WriteLine("Exports");
        //    foreach (ComposablePartDefinition cpDef in _container.Catalog.Parts.AsEnumerable())
        //    {
        //        if (cpDef.ExportDefinitions.Count() > 0)
        //        {
        //            Debug.WriteLine("{0,4}. {1}", number, cpDef);
        //            foreach (ExportDefinition exDef in cpDef.ExportDefinitions)
        //            {
        //                numberByName.Add(exDef.ContractName, number);

        //                Debug.WriteLine("        {0}", exDef.ContractName);
        //                foreach (string key in exDef.Metadata.Keys)
        //                {
        //                    Debug.WriteLine("          {0}={1}", key, exDef.Metadata[key]);
        //                }
        //            }

        //            ++number;
        //        }
        //    }
        //    foreach (ComposablePart part in batch.PartsToAdd)
        //    {
        //        if (part.ExportDefinitions.Count() > 0)
        //        {
        //            Debug.WriteLine("{0,4}. {1}", number, "batched ComposablePart");
        //            foreach (ExportDefinition exDef in part.ExportDefinitions)
        //            {
        //                numberByName.Add(exDef.ContractName, number);

        //                Debug.WriteLine("        {0}", exDef.ContractName);
        //                foreach (string key in exDef.Metadata.Keys)
        //                {
        //                    sw.WriteLine("          {0}={1}", key, exDef.Metadata[key]);
        //                }
        //            }

        //            ++number;
        //        }
        //    }

        //    Debug.WriteLine();
        //    Debug.WriteLine("Imports");
        //    foreach (ComposablePartDefinition cpDef in _container.Catalog.Parts.AsEnumerable())
        //    {
        //        if (cpDef.ImportDefinitions.Count() > 0)
        //        {
        //            Debug.WriteLine("  {0}", cpDef);
        //            foreach (ImportDefinition imDef in cpDef.ImportDefinitions)
        //            {
        //                string exportingPartNumberMessage = "missing";
        //                int exportingPartNumber;
        //                if (numberByName.TryGetValue(imDef.ContractName, out exportingPartNumber))
        //                {
        //                    exportingPartNumberMessage = exportingPartNumber.ToString();
        //                }

        //                Debug.WriteLine("    {0} ({1})", imDef.ContractName, exportingPartNumberMessage);
        //                //sw.WriteLine("      Cardinality={0}", imDef.Cardinality);
        //                //sw.WriteLine("      Constraint='{0}'", imDef.Constraint);
        //                //sw.WriteLine("      IsPrerequisite={0}", imDef.IsPrerequisite);
        //                //sw.WriteLine("      IsRecomposable={0}", imDef.IsRecomposable);
        //            }
        //        }
        //    }
        //    Debug.WriteLine();
        //    Debug.WriteLine("Metadata");
        //    foreach (ComposablePartDefinition cpDef in _container.Catalog.Parts.AsEnumerable())
        //    {
        //        if (cpDef.Metadata.Count > 0)
        //        {
        //            Debug.WriteLine("  {0}", cpDef);
        //            foreach (string key in cpDef.Metadata.Keys)
        //            {
        //                Debug.WriteLine("    {0}={1}", key, cpDef.Metadata[key]);
        //            }
        //        }
        //    }
        //}
    }
   
    /// <summary>
    /// This simple export provider searches the ServiceLocator for exports.
    /// </summary>
    sealed class ValueExportDescriptorProvider : ExportDescriptorProvider
    {
        private IServiceLocator m_ServiceLocator;

        public ValueExportDescriptorProvider(IServiceLocator serviceLocator)
            : base()
        {
            m_ServiceLocator = serviceLocator;
        }

        /// <summary>
        /// Promise export descriptors for the specified export key.
        /// </summary>
        /// <param name="contract">The export key required by another component.</param><param name="descriptorAccessor">Accesses the other export descriptors present in the composition.</param>
        /// <returns>
        /// Promises for new export descriptors.
        /// </returns>
        /// <remarks>
        /// A provider will only be queried once for each unique export key.
        /// The descriptor accessor can only be queried immediately if the descriptor being promised is an adapter, such as
        /// <see cref="T:System.Lazy`1"/>; otherwise, dependencies should only be queried within execution of the function provided
        /// to the <see cref="T:System.Composition.Hosting.Core.ExportDescriptorPromise"/>. The actual descriptors provided should not close over or reference any
        /// aspect of the dependency/promise structure, as this should be able to be GC'ed.
        /// </remarks>
        public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors(CompositionContract contract, DependencyAccessor descriptorAccessor)
        {

            if (!contract.Equals(new CompositionContract(contract.ContractType)))
                return NoExportDescriptors;

            IEnumerable<ExportDescriptorPromise> result = NoExportDescriptors;

            var providedTypes = m_ServiceLocator.RegisteredServices;

            TypeInfo contractTypeInfo = contract.ContractType.GetTypeInfo();
            var contractTypeInterfaces = contractTypeInfo.ImplementedInterfaces;
            if (contractTypeInterfaces.Contains(typeof(IService)))
            {
                foreach (Type i in contractTypeInterfaces)
                {
                }
            }
            if (!providedTypes.ContainsKey(contract.ContractType))
                return NoExportDescriptors;

            var lazyValue = providedTypes[contract.ContractType];

            Debug.WriteLine("ValueExportDescriptorProvider.GetExportDescriptors found out contract: {0}", contract.ContractType);

            return new[]
                       {
                           new ExportDescriptorPromise(contract, "ValueExportDescriptorProvider", true, NoDependencies,
                                                       _ => ExportDescriptor.Create((c, o) => lazyValue.Value, NoMetadata)),
                       };
        }
    }


    /// <summary>
    /// Default export provider
    /// </summary>
    internal sealed class DefaultExportDescriptorProvider : ExportDescriptorProvider
    {
        internal const string DefaultContractNamePrefix = "Default++";

        public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors(CompositionContract contract, DependencyAccessor descriptorAccessor)
        {
            // Avoid trying to create defaults-of-defaults-of...
            if (contract.ContractName != null && contract.ContractName.StartsWith(DefaultContractNamePrefix))
                return NoExportDescriptors;

            var implementations = descriptorAccessor.ResolveDependencies("test for default", contract, false);
            if (implementations.Any())
                return NoExportDescriptors;

            var defaultImplementationDiscriminator = DefaultContractNamePrefix + (contract.ContractName ?? "");
            IDictionary<string, object> copiedConstraints = null;
            if (contract.MetadataConstraints != null)
                copiedConstraints = contract.MetadataConstraints.ToDictionary(k => k.Key, k => k.Value);
            var defaultImplementationContract = new CompositionContract(contract.ContractType, defaultImplementationDiscriminator, copiedConstraints);

            CompositionDependency defaultImplementation;
            if (!descriptorAccessor.TryResolveOptionalDependency("default", defaultImplementationContract, true, out defaultImplementation))
                return NoExportDescriptors;

            return new[] { new ExportDescriptorPromise(
                contract,
                "Default Implementation",
                false,
                () => new[] { defaultImplementation },
                _ => {
                    var defaultDescriptor = defaultImplementation.Target.GetDescriptor();
                    return ExportDescriptor.Create((c, o) => defaultDescriptor.Activator(c, o), defaultDescriptor.Metadata);
                })};
        }
    }

    public class test : AttributedModelProvider
    {
        public override IEnumerable<Attribute> GetCustomAttributes(Type reflectedType, ParameterInfo parameter)
        {
            return new List<Attribute>();
        }

        public override IEnumerable<Attribute> GetCustomAttributes(Type reflectedType, MemberInfo member)
        {
            return new List<Attribute>();
        }
    }
}
