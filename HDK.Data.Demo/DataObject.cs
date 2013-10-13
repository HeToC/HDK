using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Deferred;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HDK.Data.Demo
{

    public interface IDeferredDataPropertyLoader<T>
    {
    }

    public abstract class DeferredDataObject : INotifyPropertyChanged, IDeferredDataItem
    {
        private Dictionary<string, object> m_InternalValues = new Dictionary<string, object>();

        public event PropertyChangedEventHandler PropertyChanged;
        public void SetValue<T>(T value, [CallerMemberName] string propertyName = null)
        {
            var behavior = GetPropertyBehavior(propertyName);
            bool valueInitialized = m_InternalValues.ContainsKey(propertyName);

            if (valueInitialized)
                m_InternalValues[propertyName] = value;
            else
                m_InternalValues.Add(propertyName, value);

            var handler = PropertyChanged;
            if (handler == null)
                return;
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            var behavior = GetPropertyBehavior(propertyName);
            bool valueInitialized = m_InternalValues.ContainsKey(propertyName);
            if (!valueInitialized)
                return default(T);

            object value = m_InternalValues[propertyName];
            return (T)value;
        }

        public DeferredDataState CurrentState
        {
            get { return GetValue<DeferredDataState>(); }
            set { SetValue<DeferredDataState>(value); }
        }

        public bool IsPaused
        {
            get { return GetValue<bool>(); }
            set { SetValue<bool>(value); }
        }

        public bool IsLoading
        {
            get { return GetValue<bool>(); }
            set { SetValue<bool>(value); }
        }

        public bool IsInUse
        {
            get { return PropertyChanged != null; }
        }

        public DeferredDataObject()
        {
            InitializePrpertyInfoCollections();
        }

        private void InitializePrpertyInfoCollections()
        {
            Type me = this.GetType();
            var allPropertsies = me.GetProperties();
            var deferredProperties = from propInfo in allPropertsies
                                     let deferredAttribute = propInfo.GetCustomAttribute<DeferredPropertyBehaviorAttribute>(false)
                                     where deferredAttribute != null
                                     select new { PropName = propInfo.Name, PropInfo = propInfo, Attr = deferredAttribute };
            m_Properties = new Dictionary<string, Tuple<PropertyInfo, DeferredPropertyBehaviorAttribute>>();
            foreach (var t in deferredProperties)
                m_Properties.Add(t.PropName, new Tuple<PropertyInfo, DeferredPropertyBehaviorAttribute>(t.PropInfo, t.Attr));

            //var mandatoryProperties = from propData in deferredProperties
            //                          where propData.Attr.IsMandatory
            //                          select propData;
            //m_mandatoryProperties = new Dictionary<string, Tuple<PropertyInfo, DeferredPropertyBehaviorAttribute>>();
            //foreach (var t in mandatoryProperties)
            //    m_mandatoryProperties.Add(t.PropName, new Tuple<PropertyInfo, DeferredPropertyBehaviorAttribute>(t.PropInfo, t.Attr));

            //var asyncProperties = from propData in deferredProperties
            //                      where !propData.Attr.IsMandatory
            //                      select propData;
            //m_asyncProperties = new Dictionary<string, Tuple<PropertyInfo, DeferredPropertyBehaviorAttribute>>();
            //foreach (var t in asyncProperties)
            //    m_mandatoryProperties.Add(t.PropName, new Tuple<PropertyInfo, DeferredPropertyBehaviorAttribute>(t.PropInfo, t.Attr));
        }
        private IDictionary<string, Tuple<PropertyInfo, DeferredPropertyBehaviorAttribute>> m_Properties;
//        private IDictionary<string, Tuple<PropertyInfo, DeferredPropertyBehaviorAttribute>> m_mandatoryProperties;
 //       private IDictionary<string, Tuple<PropertyInfo, DeferredPropertyBehaviorAttribute>> m_asyncProperties;


        private DeferredPropertyBehaviorAttribute GetPropertyBehavior(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException();

            return m_Properties.ContainsKey(propertyName) ? m_Properties[propertyName].Item2 : null;
        }
    }

    public class SampleDataObject : DeferredDataObject
    {
        [DeferredPropertyBehavior(isMandatory: true, isLazy:false, isAsync:false, isFastLoading: true, isLarge: false)]
        public int ID { get { return GetValue<int>(); } set { SetValue<int>(value); } }

        [DeferredPropertyBehavior(isMandatory: false, isLazy: true, isAsync: true, isFastLoading: true, isLarge: false)]
        public string Name { get { return GetValue<string>(); } set { SetValue<string>(value); } }

        public SampleDataObject()
        {
        }
    }
}
