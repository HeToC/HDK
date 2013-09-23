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
        private ContainerConfiguration _configuration;
        private CompositionHost _container;
        private ConventionBuilder _conventions;

        public MefCompositionProvider()
        {
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
                Conventions.ForTypesDerivedFrom<IService>().Export<IService>().Shared(); 

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
            Container.SatisfyImports(target);
        }

        public Lazy<T> GetInstance<T>() where T : class
        {
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
    }
}
