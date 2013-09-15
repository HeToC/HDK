using System;

namespace System.Services
{
    public interface ICompositionProvider
    {
        void Compose(object target);
        Lazy<object> GetInstance(Type type);
        Lazy<object> GetInstance(Type type, string contractName);
        Lazy<T> GetInstance<T>() where T : class;
        Lazy<T> GetInstance<T>(string contractName);
        System.Collections.Generic.IEnumerable<T> GetInstances<T>(string contractName);
        ICompositionFactory<T> GetInstanceFactory<T>() where T : class;
        System.Collections.Generic.IEnumerable<object> GetInstances(Type type);
        System.Collections.Generic.IEnumerable<object> GetInstances(Type type, string contractName);
        System.Collections.Generic.IEnumerable<T> GetInstances<T>();
    }
}
