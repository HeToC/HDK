using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel
{
    [DataContract(IsReference = true)]
    /// <summary> 
    /// Implementation of <see cref="INotifyPropertyChanged"/> to simplify models. 
    /// </summary> 
    public abstract class BindableBase : INotificationObject
    {
        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        /// <summary>
        /// Gets the value of a property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        protected T Get<T>([CallerMemberName] string name = null)
        {
            Debug.Assert(name != null, "name != null");
            object value = null;
            if (_properties.TryGetValue(name, out value))
                return value == null ? default(T) : (T)value;
            return default(T);
        }

        /// <summary>
        /// Sets the value of a property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <remarks>Use this overload when implicitly naming the property</remarks>
        protected void Set<T>(T value, [CallerMemberName] string name = null)
        {
            Debug.Assert(name != null, "name != null");
            if (Equals(value, Get<T>(name)))
                return;
            _properties[name] = value;
            RaisePropertyChanged(name);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] String propertyName = null)
        {
            this.RaisePropertyChanged(PropertyChanged, propertyName);
        }

        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            INotifyPropertyChangedExtensions.RaisePropertyChanged(this, PropertyChanged, propertyNames);
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            return PropertyChanged.SetPropertyValueAndNotify(this, ref storage, value, propertyName);
        }
    } 

}
