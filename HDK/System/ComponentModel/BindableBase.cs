using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel
{
    [global::Windows.Foundation.Metadata.WebHostHidden]
    [DataContract(IsReference = true)]
    public abstract class BindableBase : INotificationObject
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] String propertyName = null)
        {
            this.RaisePropertyChanged(PropertyChanged, propertyName);
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            return PropertyChanged.SetPropertyValueAndNotify(this, ref storage, value, propertyName);
        }
    }
}
