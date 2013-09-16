using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    public class AsyncLazy<T> : INotifyPropertyChanged
    {
        #region Fields

        readonly object locker = new object();
        T m_DefaultValue;
        Lazy<Task<T>> m_Adaptee;
        private string m_ErrorMessage = null;
        private bool m_IsFaulted = false;
        private bool m_HasValue = false;
        private AsyncLazyStatus m_Status = AsyncLazyStatus.NotInitialized;

        #endregion Fields

        #region Constructors

        public AsyncLazy(Func<T> valueFactory, T defaultValue = default(T))
        {
            if (valueFactory == null)
            {
                throw new ArgumentNullException("valueFactory");
            }

            m_DefaultValue = defaultValue;
            m_Adaptee = new Lazy<Task<T>>(() => AppendTask(Task.Run(valueFactory)));
        }

        public AsyncLazy(Func<Task<T>> taskFactory, T defaultValue = default(T))
        {
            if (taskFactory == null)
            {
                throw new ArgumentNullException("taskFactory");
            }

            m_DefaultValue = defaultValue;
            m_Adaptee = new Lazy<Task<T>>(() => AppendTask(Task.Run(taskFactory)));
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public T Value
        {
            get
            {
                if (IsFaulted)
                    return m_DefaultValue;

                if (m_Adaptee.Value.IsCompleted)
                    return m_Adaptee.Value.Result;
                else
                    lock (locker)
                        return m_DefaultValue;
            }
            private set
            {
                PropertyChanged.SetPropertyValueAndNotify(this, ref m_DefaultValue, value);
            }
        }

        public AsyncLazyStatus Status
        {
            get { return m_Status; }
            private set { PropertyChanged.SetPropertyValueAndNotify(this, ref m_Status, value); }
        }

        public string ErrorMessage
        {
            get { return m_ErrorMessage; }
            private set { PropertyChanged.SetPropertyValueAndNotify(this, ref m_ErrorMessage, value); }
        }

        public bool IsFaulted
        {
            get { return m_IsFaulted; }
            private set { PropertyChanged.SetPropertyValueAndNotify(this, ref m_IsFaulted, value); }
        }

        public bool HasValue
        {
            get { return m_HasValue; }
            private set { PropertyChanged.SetPropertyValueAndNotify(this, ref m_HasValue, value); }
        }

        #endregion Properties

        #region Methods

        public TaskAwaiter<T> GetAwaiter()
        {
            return m_Adaptee.Value.GetAwaiter();
        }


        Task<T> AppendTask(Task<T> task)
        {
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    if (t.Exception is AggregateException)
                        ErrorMessage = ((AggregateException)t.Exception).InnerExceptions[0].Message;
                    else
                        ErrorMessage = t.Exception.Message;
                    HasValue = IsFaulted = true;
                }
                else
                {
                    lock (locker)
                    {
                        Value = t.Result;
                    }
                    HasValue = true;
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());

            return task;
        }

        #endregion Methods
    }
}
