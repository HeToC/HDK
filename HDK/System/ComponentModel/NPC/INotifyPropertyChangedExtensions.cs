using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel
{
    public static class INotifyPropertyChangedExtensions
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> PropertyChangedEventArgsCache = new ConcurrentDictionary<string, PropertyChangedEventArgs>();
        private static readonly Func<string, PropertyChangedEventArgs> PropertyChangedEventArgsFactory = propertyName =>
            {
                PropertyChangedEventArgs ret = null;
                if (PropertyChangedEventArgsCache.TryGetValue(propertyName, out ret))
                    return ret;

                PropertyChangedEventArgsCache.TryAdd(propertyName, ret = new PropertyChangedEventArgs(propertyName));
                return ret;
            };


        public static bool SetPropertyValueAndNotify<T>(this PropertyChangedEventHandler handler, INotifyPropertyChanged sender, ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;

            handler.Raise(sender, propertyName);
            return true;
        }


        public static void Raise(this PropertyChangedEventHandler handler, INotifyPropertyChanged sender, [CallerMemberName] string propertyName = null)
        {
            if (handler != null && !string.IsNullOrEmpty(propertyName))
            {
                PropertyChangedEventArgs args = PropertyChangedEventArgsFactory(propertyName);
                handler(sender, args);
            }
        }


        public static void RaisePropertyChanged(this INotifyPropertyChanged sender, PropertyChangedEventHandler handler, [CallerMemberName] string propertyName = null)
        {
            if (handler != null && !string.IsNullOrEmpty(propertyName))
            {
                PropertyChangedEventArgs args = PropertyChangedEventArgsFactory(propertyName);
                handler(sender, args);
            }
        }
        public static void RaisePropertyChanged(this INotifyPropertyChanged sender, PropertyChangedEventHandler handler, params string[] propertyNames)
        {
            if (handler != null && propertyNames != null && propertyNames.Length>0)
            {
                foreach (var propertyName in propertyNames)
                    sender.RaisePropertyChanged(handler, propertyName);
            }
        }
    }
}
