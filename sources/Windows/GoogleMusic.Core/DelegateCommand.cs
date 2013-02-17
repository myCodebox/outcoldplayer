// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// The delegate command.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        public DelegateCommand(Action execute)
            : this(o => execute())
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
        }

        public DelegateCommand(Action<object> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            this.execute = execute;
        }

        public DelegateCommand(Action execute, Func<bool> canExecute)
            : this(o => execute(), (o) => canExecute())
        {
            if (canExecute == null)
            {
                throw new ArgumentNullException("canExecute");
            }
        }

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
            : this(execute)
        {
            if (canExecute == null)
            {
                throw new ArgumentNullException("canExecute");
            }

            this.canExecute = canExecute;
        }

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged;

        /// <inheritdoc />
        public bool CanExecute(object parameter = null)
        {
            if (this.canExecute == null)
            {
                return true;
            }

            return this.canExecute(parameter);
        }

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            if (this.CanExecute(null))
            {
                this.execute(parameter);
            }
        }

        /// <inheritdoc />
        public void RaiseCanExecuteChanged()
        {
            var handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}