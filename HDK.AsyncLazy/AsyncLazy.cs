using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    /// <summary>
    /// TODO: Test using weak reference
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncLazy<T> : INotifyPropertyChanged
    {
        #region Constructor default attribute values

        private static readonly CancellationToken DefaultCancellationToken = CancellationToken.None;
        private static readonly TaskCreationOptions DefaultCreationOptions = TaskCreationOptions.None;
        private static readonly TaskScheduler DefaultTaskScheduler = TaskScheduler.Default;

        #endregion

        #region Private variables
        private readonly Lazy<Task<T>> m_adaptee;
        private T m_DefaultValue = default(T);
        private T m_Value = default(T);
        private Task<T> m_adoptedTask { get { return (m_adaptee != null) ? m_adaptee.Value : null; } }
        #endregion

        #region Public Properties

        public bool IsValueCreated { get { return (m_adaptee != null) ? m_adaptee.IsValueCreated : false; } }

        //
        // Summary:
        //     Gets the System.AggregateException that caused the System.Threading.Tasks.m_adoptedTask
        //     to end prematurely. If the System.Threading.Tasks.m_adoptedTask completed successfully
        //     or has not yet thrown any exceptions, this will return null.
        //
        // Returns:
        //     The System.AggregateException that caused the System.Threading.Tasks.m_adoptedTask
        //     to end prematurely.
        public AggregateException Exception { get { return this.IsValueCreated ? m_adoptedTask.Exception : null; } }

        public Exception InnerException { get { return (this.Exception == null) ? null : this.Exception.InnerException; } }

        public string ErrorMessage { get { return (this.InnerException == null) ? null : this.InnerException.Message; } }

        //
        // Summary:
        //     Gets a unique ID for this System.Threading.Tasks.m_adoptedTask instance.
        //
        // Returns:
        //     An integer that was assigned by the system to this task instance.
        public int TaskId { get { return this.IsValueCreated ? m_adoptedTask.Id : -1; } }

        //
        // Summary:
        //     Gets whether this System.Threading.Tasks.m_adoptedTask instance has completed execution
        //     due to being canceled.
        //
        // Returns:
        //     true if the task has completed due to being canceled; otherwise false.
        public bool IsCanceled { get { return this.IsValueCreated ? m_adoptedTask.IsCanceled : false; } }

        //
        // Summary:
        //     Gets whether this System.Threading.Tasks.m_adoptedTask has completed.
        //
        // Returns:
        //     true if the task has completed; otherwise false.
        public bool IsCompleted { get { return this.IsValueCreated ? m_adoptedTask.IsCompleted : false; } }

        public bool IsSuccessfullyCompleted { get { return this.Status == AsyncLazyStatus.RanToCompletion; } }

        //
        // Summary:
        //     Gets whether the System.Threading.Tasks.m_adoptedTask completed due to an unhandled
        //     exception.
        //
        // Returns:
        //     true if the task has thrown an unhandled exception; otherwise false.
        public bool IsFaulted { get { return this.IsValueCreated ? m_adoptedTask.IsFaulted : false; } }

        //
        // Summary:
        //     Gets the System.Threading.Tasks.TaskStatus of this m_adoptedTask.
        //
        // Returns:
        //     The current System.Threading.Tasks.TaskStatus of this task instance.
        public AsyncLazyStatus Status { get { return (AsyncLazyStatus)(this.IsValueCreated ? (int)m_adoptedTask.Status : -1); } }

        public T DefaultValue
        {
            get
            {
                return m_DefaultValue;
            }
            set
            {
                m_DefaultValue = value;
                this.InvokePropertyChanged();
            }
        }

        public T Value
        {
            get
            {
                if (m_adaptee.Value.IsCompleted)
                    return m_adaptee.Value.Result;

                return DefaultValue;
            }
            set
            {
                m_Value = value;

                this.InvokePropertyChanged();
            }
        }

        #endregion

        #region Constructors

        #region Value factory based

        public AsyncLazy(Func<T> valueFactory, T defaultValue = default(T)) :
            this(valueFactory, defaultValue, DefaultCancellationToken, DefaultCreationOptions, DefaultTaskScheduler)
        {
        }

        public AsyncLazy(Func<T> valueFactory, CancellationToken cancellationToken, T defaultValue = default(T)) :
            this(valueFactory, defaultValue, cancellationToken, DefaultCreationOptions, DefaultTaskScheduler)
        {
        }

        public AsyncLazy(Func<T> valueFactory, TaskCreationOptions creationOptions, T defaultValue = default(T)) :
            this(valueFactory, defaultValue, DefaultCancellationToken, creationOptions, DefaultTaskScheduler)
        {
        }


        #endregion

        #region Task factory based

        public AsyncLazy(Func<Task<T>> taskFactory, T defaultValue = default(T)) :
            this(taskFactory, defaultValue, DefaultCancellationToken, DefaultCreationOptions, DefaultTaskScheduler)
        {
        }

        public AsyncLazy(Func<Task<T>> taskFactory, CancellationToken cancellationToken, T defaultValue = default(T)) :
            this(taskFactory, defaultValue, cancellationToken, DefaultCreationOptions, DefaultTaskScheduler)
        {
        }

        public AsyncLazy(Func<Task<T>> taskFactory, TaskCreationOptions creationOptions, T defaultValue = default(T)) :
            this(taskFactory, defaultValue, DefaultCancellationToken, creationOptions, DefaultTaskScheduler)
        {
        }

        #endregion

        #region Core constructors

        public AsyncLazy(Func<T> valueFactory, T defaultValue, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            DefaultValue = defaultValue;

            m_adaptee = CreateAdaptee(valueFactory, null, cancellationToken, creationOptions, scheduler);
        }

        public AsyncLazy(Func<object, T> valueFactory,T defaultValue, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            DefaultValue = defaultValue;

            m_adaptee = CreateAdaptee(valueFactory, state, cancellationToken, creationOptions, scheduler);
        }

        public AsyncLazy(Func<Task<T>> taskFactory, T defaultValue, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            DefaultValue = defaultValue;

            m_adaptee = CreateAdaptee(() => taskFactory(), null, cancellationToken, creationOptions, scheduler);
        }

        public AsyncLazy(Func<object, Task<T>> taskFactory,T defaultValue, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            DefaultValue = defaultValue;

            m_adaptee = CreateAdaptee((o) => taskFactory(o), state, cancellationToken, creationOptions, scheduler);
        }

        #endregion

        #endregion

        #region Adaptee Factory

        private Lazy<Task<T>> CreateAdaptee(Func<T> valueFactory, object context, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return new Lazy<Task<T>>(() => Task.Factory.StartNew(valueFactory, cancellationToken, creationOptions, scheduler).ContinueWith<T>(TaskContinuationFunction));
        }
        private Lazy<Task<T>> CreateAdaptee(Func<object, T> valueFactory, object context, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return new Lazy<Task<T>>(() => Task.Factory.StartNew(valueFactory, context, cancellationToken, creationOptions, scheduler).ContinueWith<T>(TaskContinuationFunction));
        }

        private Lazy<Task<T>> CreateAdaptee(Func<Task<T>> taskFactory, object context, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return new Lazy<Task<T>>(() => Task.Factory.StartNew<Task<T>>(() => taskFactory(), cancellationToken, creationOptions, scheduler).Unwrap().ContinueWith<T>(TaskContinuationFunction));
        }
        private Lazy<Task<T>> CreateAdaptee(Func<object, Task<T>> taskFactory, object context, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return new Lazy<Task<T>>(() => Task.Factory.StartNew<Task<T>>(taskFactory, context, cancellationToken, creationOptions, scheduler)
                .Unwrap().ContinueWith<T>(TaskContinuationFunction));
        }

        //protected virtual Lazy<Task<T>> CreateAdaptee(Expression<Func<T>> factory, object context, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        //{
        //    if(factory is Func<T>)
        //        return new Lazy<Task<T>>(() => Task.Factory.StartNew<T>(factory as Func<T>, cancellationToken, creationOptions, scheduler)
        //            .ContinueWith<T>(TaskContinuationFunction));
        //    else if (factory is Func<object, T>)
        //        return new Lazy<Task<T>>(() => Task.Factory.StartNew<T>(factory as Func<object, T>, context, cancellationToken, creationOptions, scheduler)
        //            .ContinueWith<T>(TaskContinuationFunction));
        //    else if (factory is Func<Task<T>>)
        //        return new Lazy<Task<T>>(
        //            () => 
        //                Task.Factory.StartNew<T>(
        //                    (factory as Func<Task<T>>)(),
        //                    cancellationToken,
        //                    creationOptions,
        //                    scheduler)
        //            .Unwrap()
        //            .ContinueWith<T>(TaskContinuationFunction));
        //    else if (factory is Func<object, Task<T>>)
        //        return new Lazy<Task<T>>(() => Task.Factory.StartNew<T>(factory as Func<object, Task<T>>, context, cancellationToken, creationOptions, scheduler).Unwrap()
        //            .ContinueWith<T>(TaskContinuationFunction));

        //    throw new ArgumentException("Argumernt is neither Func<T> nor Func<object,T>", "factory");
        //}

        #endregion

        protected virtual T TaskContinuationFunction(Task<T> task)
        {
            if (!task.IsCompleted)
            {
                var scheduler = (SynchronizationContext.Current == null) ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
                task.ContinueWith(t =>
                {
                    this.InvokePropertyChanged("Status", "IsCompleted", "IsValueCreated");
                    if (t.IsCanceled)
                        this.InvokePropertyChanged("IsCanceled");
                    else if (t.IsFaulted)
                        this.InvokePropertyChanged("IsFaulted", "Exception", "InnerException", "ErrorMessage");
                    else
                    {
                        Value = t.Result;
                        this.InvokePropertyChanged("IsSuccessfullyCompleted", "Result", "Value");
                    }

                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                scheduler);
            }

            return task.Result;
        }

        public async void Start()
        {
            await this;
        }

        public TaskAwaiter<T> GetAwaiter() { return m_adoptedTask.GetAwaiter(); }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> 
        /// Notifies listeners that a property value has changed. 
        /// </summary> 
        /// <param name="propertyName">Name of the property used to notify listeners.  This 
        /// value is optional and can be provided automatically when invoked from compilers 
        /// that support <see cref="CallerMemberNameAttribute"/>.</param> 
        protected void InvokePropertyChanged([CallerMemberName] string primaryPropertyName = null, params string[] secondaryPropertyNames)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(primaryPropertyName));

                foreach (string propertyName in secondaryPropertyNames)
                    eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
