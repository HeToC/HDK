using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Services;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// Interface that defines Add/Remove and typesafe Resolve
    /// inspired http://searchcode.com/codesearch/view/10571311
    /// </summary>
    public interface IServiceLocator : IServiceProvider
    {
        /// <summary>
        /// Returns whether the service exists.
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>True/False</returns>
        bool Exists(Type type);

        /// <summary>
        /// Adds a new service to the resolver list
        /// </summary>
        /// <param name="type">Service Type (typically an interface)</param>
        /// <param name="valueFactory">Object that implements service</param>
        /// <param name="options">Conflict resolving options</param>
        void Add(Type type, Func<object> valueFactory, ConflictResolvingOptions options);

        /// <summary>
        /// This adds a new service to the resolver list.
        /// </summary>
        /// <typeparam name="T">Type of the service</typeparam>
        /// <param name="valueFactory">Value</param>
        /// <param name="options">Conflict resolving options</param>
        void Add<T>(Func<T> valueFactory, ConflictResolvingOptions options);

        /// <summary>
        /// Remove a service
        /// </summary>
        /// <param name="type">Type to remove</param>
        void Remove(Type type);

        /// <summary>
        /// This resolves a service type and returns the implementation. Note that this
        /// assumes the key used to register the object is of the appropriate type or
        /// this method will throw an InvalidCastException!
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <returns>Implementation</returns>
        T Resolve<T>();

        /// <summary>
        /// This returns all the registered services
        /// </summary>
        IReadOnlyDictionary<Type, Lazy<object>> RegisteredServices { get; }

        //void Populate(ICompositionProvider compositionProvider);
    }
}
