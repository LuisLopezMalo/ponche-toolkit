using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PoncheToolkit.EffectsCreator.Commands
{
    /// <summary>
    /// Class to be inherited for any command sent between UI and Code behind.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private Action<T> execute;
        private Predicate<T> canExecute;

        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Constructor.
        /// Set by default to true the canExecute action.
        /// </summary>
        /// <param name="execute"></param>
        public RelayCommand(Action<T> execute) 
            : this(execute, p => true)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="execute">The Function to be executed for this command.</param>
        /// <param name="canExecute">The Function to know if this command can be executed.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <inheritdoc/>
        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute((T)parameter);
        }

        /// <inheritdoc/>
        public void Execute(object parameter)
        {
            this.execute((T)parameter);
        }
    }
}
