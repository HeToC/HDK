using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace System.Data
{
    public abstract class DataObject : INotifyPropertyChanged
    {
        internal const string ObjectContextName = "Context";
        private DataObjectSet _context;
        public DataObjectSet Context
        {
            get { return _context; }
            internal set
            {
                if (value != _context)
                {
                    _context = value;
                    RaisePropertyChanged(ObjectContextName);
                }
            }
        }

        public string ObjectId
        {
            get { return GetType().Name + "" + Id; }

        }

        private long _id;

        protected DataObject()
        {
            _context = null;
        }

        protected DataObject(DataObjectSet context, long id)
        {
            _context = context;
            Id = id;
        }

        public long Id
        {
            get { return _id; }
            private set
            {
                if (_id == value) return;
                _id = value;
                RaisePropertyChanged("Id");
            }
        }

        public void Delete()
        {
            Context = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var ev = PropertyChanged;
            if (ev != null)
            {
                ev(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Copy<T>(T source) where T : DataObject
        {
            Id = source.Id;
            var propertyInfos = typeof(T).GetRuntimeProperties();
            foreach (var property in propertyInfos)
            {
                if (!property.CanWrite || property.Name == ObjectContextName || !property.CanRead) continue;
                var value = property.GetValue(source, null);
                property.SetValue(this, value, null);
            }
        }

        protected T FindInContext<T>(long id) where T : DataObject
        {
            var context = Context;
            if (context == null) return null;
            return context.FindEntity<T>(id);
        }
    }
}
