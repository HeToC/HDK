using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Services;
using System.Text;
using System.Threading.Tasks;

namespace System
{

    public enum ConflictResolvingOptions
    {
        Skip,
        Replace,
        Throw
    }

    public class ServiceLocatorRegistrationException : Exception
    {
        public Type ServiceType { get; private set; }
        public ServiceLocatorRegistrationException(string message, Type serviceType)
            : base(message)
        {
            ServiceType = serviceType;
        }
    }

    /// <summary>
    /// This class acts as a resolver for typed services (interfaces and implementations).
    /// Internally it relies on an IServiceContainer - it will create a BCL version if one is not 
    /// supplied.  Any custom implementation can also be used - this provider will not use the 
    /// promotion features so those do not need to be implemented.
    /// </summary>
    /// <example>
    /// To register a service use Add:
    /// <![CDATA[
    /// serviceResolver.Add(typeof(IService), new Service());
    /// 
    /// To retrieve a service use Resolve:
    /// 
    /// IService svc = serviceResolver<IService>.Resolve();
    /// ]]>
    /// </example>
    [Shared]
    //[Export(typeof(IServiceLocator))]
    public sealed class ServiceLocator : IServiceLocator
    {
        [ImportMany]
        IEnumerable<Lazy<IService, ServiceMetadata>> ComposedServices { get; set; }

        public ServiceLocator()
        {
        }

        /// <summary>
        /// Lock
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Returns whether the service exists.
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>True/False</returns>
        public bool Exists(Type type)
        {
            lock (_lock)
            {
                // Quick check of the container.
                if (m_serviceContainer.ContainsKey(type))
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Service container
        /// </summary>
        private readonly Dictionary<Type, Lazy<object>> m_serviceContainer = new Dictionary<Type, Lazy<object>>();

        /// <summary>
        /// Adds a new service to the resolver list
        /// </summary>
        /// <param name="type">Service Type (typically an interface)</param>
        /// <param name="valueFactory">Object that implements service</param>
        public void Add(Type type, Func<object> valueFactory, ConflictResolvingOptions options = ConflictResolvingOptions.Skip)
        {
            lock (_lock)
            {
                if (Exists(type))
                {
                    switch (options)
                    {
                        case ConflictResolvingOptions.Replace:
                            Remove(type);
                            break;
                        case ConflictResolvingOptions.Skip:
                            return;
                        case ConflictResolvingOptions.Throw:
                            throw new ServiceLocatorRegistrationException("Registering existing service disabled due to ConflictResolvingOptions value",type);
                    }
                }
                m_serviceContainer.Add(type, new Lazy<object>(valueFactory));
            }
        }

        /// <summary>
        /// This adds a new service to the resolver list.
        /// </summary>
        /// <typeparam name="T">Type of the service</typeparam>
        /// <param name="value">Value</param>
        public void Add<T>(Func<T> valueFactory, ConflictResolvingOptions options = ConflictResolvingOptions.Skip)
        {
            Add(typeof(T), () => valueFactory(), options);
        }

        /// <summary>
        /// Remove a service
        /// </summary>
        /// <param name="type">Type to remove</param>
        public void Remove(Type type)
        {
            lock (_lock)
            {
                if (m_serviceContainer != null)
                {
                    if (m_serviceContainer.ContainsKey(type))
                        m_serviceContainer.Remove(type);
                }
            }
        }

        /// <summary>
        /// This resolves a service type and returns the implementation. Note that this
        /// assumes the key used to register the object is of the appropriate type or
        /// this method will throw an InvalidCastException!
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <returns>Implementation</returns>
        public T Resolve<T>()
        {
            return (T)GetService(typeof(T));
        }

        /// <summary>
        /// This returns all the registered services
        /// </summary>
        public IReadOnlyDictionary<Type, Lazy<object>> RegisteredServices
        {
            get
            {
                lock (_lock)
                {
                    return m_serviceContainer != null
                               ? m_serviceContainer.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                               : new Dictionary<Type, Lazy<object>>();
                }
            }
        }

        /// <summary>
        /// Implementation of IServiceProvider
        /// </summary>
        /// <param name="serviceType">Service Type</param>
        /// <returns>Object implementing service</returns>
        public object GetService(Type serviceType)
        {
            lock (_lock)
            {
                Lazy<object> returningObject;
                return m_serviceContainer.TryGetValue(serviceType, out returningObject)
                    ? returningObject.Value
                    : null;
            }
        }

        [OnImportsSatisfied]
        internal void CompositionCompleted()
        {
            foreach (var ef in this.ComposedServices)
                this.Add(ef.Metadata.ServiceType, () => ef.Value, ConflictResolvingOptions.Skip);
        }
    }


    /*

    //public class MEFServiceLocator : ServiceLocator
    //{
    //    private ContainerConfiguration _configuration;
    //    private CompositionHost _container;
    //    private ConventionBuilder _conventions;


    //    public event Func<ICompositionProvider, ContainerConfiguration, List<Assembly>, bool> PreCreateContainer;

    //    public event Action<ICompositionProvider, ContainerConfiguration> CreatingContainer;

    //    private ConventionBuilder Conventions
    //    {
    //        get { return _conventions ?? (_conventions = new ConventionBuilder()); }
    //    }

    //    public ContainerConfiguration Configuration
    //    {
    //        get
    //        {
    //            if (_configuration != null)
    //                return _configuration;

    //            // Add conventions.
    //            Conventions.ForTypesDerivedFrom<IService>().Export<IService>().Shared(); //TODO BUG Dont work properly
    //            //Conventions.ForTypesDerivedFrom<IServiceLocator>().Export<IServiceLocator>().Shared();


    //            // Get a list of all the package assemblies.
    //            var assemblies = Task.Run(async () =>
    //            {
    //                var localAssemblies = new List<Assembly>();
    //                var result = await GetPackageAssemblyListAsync();
    //                var theAsms = result.ToList();
    //                foreach (var newAsm in theAsms.Where(asm => !localAssemblies.Contains(asm)
    //                    && !asm.FullName.StartsWith("System.Composition")))
    //                    localAssemblies.Add(newAsm);
    //                return localAssemblies;
    //            }).Result;


    //            _configuration = new ContainerConfiguration();

    //            _configuration = _configuration.WithProvider(new ValueExportDescriptorProvider(this));

    //            //_configuration = _configuration.WithProvider(new InheritedExportDescriptorProvider());

    //            // Let any customization occur
    //            if (PreCreateContainer == null || !PreCreateContainer(this, _configuration, assemblies))
    //                _configuration = _configuration.WithAssemblies(assemblies, Conventions);

    //            if (!assemblies.Contains(GetType().GetTypeInfo().Assembly))
    //                _configuration = _configuration.WithAssembly(GetType().GetTypeInfo().Assembly, Conventions);

    //            return _configuration;
    //        }
    //    }

    //    public CompositionHost Container
    //    {
    //        get
    //        {
    //            if (CreatingContainer != null)
    //                CreatingContainer(this, Configuration);
    //            return _container ?? (_container = Configuration.CreateContainer());
    //        }
    //    }

    //    public override void Compose(object target)
    //    {
    //        try
    //        {
    //            Container.SatisfyImports(target);
    //        }
    //        catch (Exception exc)
    //        {
    //        }
    //    }

    //    public override Lazy<T> GetInstance<T>()
    //    {

    //        return new Lazy<T>(() => Container.GetExport<T>());
    //    }

    //    public override Lazy<T> GetInstance<T>(string contractName)
    //    {
    //        return new Lazy<T>(() => Container.GetExport<T>(contractName));
    //    }

    //    public override IEnumerable<T> GetInstances<T>()
    //    {
    //        return Container.GetExports<T>();
    //    }

    //    public override Lazy<object> GetInstance(Type type)
    //    {
    //        return new Lazy<object>(() => Container.GetExport(type));
    //    }

    //    public override Lazy<object> GetInstance(Type type, string contractName)
    //    {
    //        return new Lazy<object>(() => Container.GetExport(type, contractName));
    //    }

    //    public override IEnumerable<object> GetInstances(Type type)
    //    {
    //        return Container.GetExports(type);
    //    }

    //    public override IEnumerable<object> GetInstances(Type type, string contractName)
    //    {
    //        return Container.GetExports(type, contractName);
    //    }
    //    public override IEnumerable<T> GetInstances<T>(string contractName)
    //    {
    //        return Container.GetExports<T>(contractName);
    //    }

    //    public override ICompositionFactory<T> GetInstanceFactory<T>()
    //    {
    //        var factory = new MefCompositionFactory<T>();
    //        Container.SatisfyImports(factory);
    //        if (factory.ExportFactory == null)
    //            throw new CompositionFailedException();

    //        return factory;
    //    }

    //    /// <summary>
    //    /// This method retrieves all the assemblies in the current app package
    //    /// </summary>
    //    /// <returns></returns>
    //    internal async static Task<IEnumerable<Assembly>> GetPackageAssemblyListAsync()
    //    {
    //        var installFolder = global::Windows.ApplicationModel.Package.Current.InstalledLocation;

    //        // If we are in the designer, then load all subdirectories which have DLLs too.
    //        // This allows the ViewModelLocator and ServiceLocator to find elements properly
    //        // when being shadow copied
    //        if (Designer.InDesignMode)
    //        {
    //            // Grab all the possibilities
    //            var assemblies = new List<Assembly>();
    //            foreach (var folder in await installFolder.GetFoldersAsync())
    //            {
    //                assemblies.AddRange((await folder.GetFilesAsync())
    //                        .Where(file => file.FileType == ".dll" || file.FileType == ".exe")
    //                        .Select(file => file.Name.Substring(0, file.Name.Length - file.FileType.Length))
    //                        .Distinct()
    //                        .Select(asmName => Assembly.Load(new AssemblyName(asmName))));
    //            }
    //            return assemblies;
    //        }

    //        // Otherwise we just look in the package folder.
    //        return ((await installFolder.GetFilesAsync())
    //            .Where(file => file.FileType == ".dll" || file.FileType == ".exe")
    //            .Select(file => file.Name.Substring(0, file.Name.Length - file.FileType.Length))
    //            .Select(asmName => Assembly.Load(new AssemblyName(asmName))))
    //            .ToList();
    //    }

    //    /// <summary>
    //    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    //    /// </summary>
    //    public void Dispose()
    //    {
    //        if (_container != null)
    //        {
    //            _container.Dispose();
    //            _container = null;
    //        }

    //        _container = null;
    //    }
    //}     * 
     */
}
