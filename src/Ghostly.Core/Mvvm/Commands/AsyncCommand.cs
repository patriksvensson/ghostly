using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ghostly.Core.Mvvm.Commands
{
    public abstract class AsyncCommand : IAsyncCommand
    {
        public event EventHandler CanExecuteChanged;
        private readonly IErrorHandler _errorHandler;
        private bool _isExecuting;

        protected AsyncCommand(IErrorHandler errorHandler = null)
        {
            _errorHandler = errorHandler;
        }

        protected abstract Task ExecuteCommand();

        public bool CanExecute()
        {
            return !_isExecuting && CanExecuteCommand();
        }

        protected virtual bool CanExecuteCommand()
        {
            return true;
        }

        public async Task ExecuteAsync()
        {
            if (CanExecute())
            {
                try
                {
                    _isExecuting = true;
                    await ExecuteCommand();
                }
                finally
                {
                    _isExecuting = false;
                }
            }

            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        void ICommand.Execute(object parameter)
        {
            ExecuteAsync().FireAndForgetSafeAsync(_errorHandler);
        }
    }

    public abstract class AsyncCommand<T> : IAsyncCommand<T>
    {
        public event EventHandler CanExecuteChanged;
        private readonly IErrorHandler _errorHandler;
        private bool _isExecuting;

        protected AsyncCommand(IErrorHandler errorHandler = null)
        {
            _errorHandler = errorHandler;
        }

        protected abstract Task ExecuteCommand(T parameter);

        public bool CanExecute(T parameter)
        {
            return !_isExecuting && CanExecuteCommand(parameter);
        }

        protected virtual bool CanExecuteCommand(T parameter)
        {
            return true;
        }

        public async Task ExecuteAsync(T parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    await ExecuteCommand(parameter);
                }
                finally
                {
                    _isExecuting = false;
                }
            }

            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            ExecuteAsync((T)parameter).FireAndForgetSafeAsync(_errorHandler);
        }
    }
}
