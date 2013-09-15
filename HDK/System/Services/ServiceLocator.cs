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
    [Export(typeof(IServiceLocator))]
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
        protected void CompositionCompleted()
        {
            foreach (var ef in this.ComposedServices)
                this.Add(ef.Metadata.ServiceType, () => ef.Value, ConflictResolvingOptions.Skip);
        }
    }
}
