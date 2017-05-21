using System;
using System.Windows.Input;

namespace VP_RegApplication.MVVM
{
    /// <summary>
    /// ICommand implementation
    /// </summary>
    public class RelayCommand : ICommand
    {
        private Action methodToExecute;
        private Func<bool> canExecuteEvaluator;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="methodToExecute"></param>
        /// <param name="canExecuteEvaluator"></param>
        public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator)
        {
            this.methodToExecute = methodToExecute;
            this.canExecuteEvaluator = canExecuteEvaluator;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="methodToExecute"></param>
        public RelayCommand(Action methodToExecute)
            : this(methodToExecute, null) { }

        #region ICommand Members
        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="methodToExecute">
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null
        /// </param>
        /// <returns type="bool">
        /// true if this command can be executed; otherwise, false.
        /// </returns>     
        public bool CanExecute(object parameter)
        {
            if (this.canExecuteEvaluator == null)
            {
                return true;
            }
            else
            {
                bool result = this.canExecuteEvaluator.Invoke();
                return result;
            }
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="methodToExecute">
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            this.methodToExecute.Invoke();
        }
        #endregion
    }
}
