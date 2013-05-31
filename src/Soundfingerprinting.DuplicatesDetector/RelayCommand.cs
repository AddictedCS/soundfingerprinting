namespace Soundfingerprinting.DuplicatesDetector
{
    using System;
    using System.Windows.Input;

    /// <summary>
    ///   Relay command class
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields that will actually be used to execute the code

        /// <summary>
        ///   Method which verifies whether the command can execute
        /// </summary>
        private readonly Predicate<object> _canExecute;

        /// <summary>
        ///   Method to execute
        /// </summary>
        private readonly Action<object> _execute;

        #endregion

        #region Constructors

        /// <summary>
        ///   Relay command constructor
        /// </summary>
        /// <param name = "execute">Method to be executed</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        ///   Relay command constructor
        /// </summary>
        /// <param name = "execute">Method to be executed</param>
        /// <param name = "canExecute">Method to check whether execution is allowed</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

        /// <summary>
        ///   Check if the method can be executed
        /// </summary>
        /// <param name = "parameter">Parameter</param>
        /// <returns>True/False</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute.Invoke(parameter);
        }

        /// <summary>
        ///   Fires when the CanExecute status of this command changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        ///   Execute the method
        /// </summary>
        /// <param name = "parameter">Parameter for execution</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion
    }
}