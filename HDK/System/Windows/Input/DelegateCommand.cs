using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Windows.Input
{
    public class DelegateCommand : ICommand
    {
        protected readonly Predicate<object> _canExecute;
        protected Func<object, Task> _asyncExecute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action syncExecute, Predicate<object> canExecute = null)
        {
            _asyncExecute = (parameter) => Task.Factory.StartNew(syncExecute, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            _canExecute = canExecute;
        }

        public DelegateCommand(Action<object> syncExecute, Predicate<object> canExecute = null)
        {
            _asyncExecute = (parameter) => Task.Factory.StartNew(syncExecute, parameter, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            _canExecute = canExecute;
        }

        public DelegateCommand(Func<object, Task> asyncExecute, Predicate<object> canExecute = null)
        {
            _asyncExecute = asyncExecute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter);
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        protected virtual async Task ExecuteAsync(object parameter)
        {
            await _asyncExecute(parameter);
        }

        //TODO: Make it protected later and raise this event on proper condition occured
        public void RaiseCanExecuteChanged()
        {
            var handler = this.CanExecuteChanged;

            if (handler != null)
                handler(this, new EventArgs());
        }

    }
    public class DelegateCommandSync : ICommand
    {
        protected readonly Predicate<object> _canExecute;
        protected Action _asyncExecute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommandSync(Action syncExecute, Predicate<object> canExecute = null)
        {
            _asyncExecute = syncExecute;
            _canExecute = canExecute;
        }


        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _asyncExecute();
        }


        //TODO: Make it protected later and raise this event on proper condition occured
        public void RaiseCanExecuteChanged()
        {
            var handler = this.CanExecuteChanged;

            if (handler != null)
                handler(this, new EventArgs());
        }

    }
}
