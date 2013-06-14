namespace SoundFingerprinting.DuplicatesDetector
{
    using System;
    using System.Windows.Input;

    public class RelayCommand : ICommand
    {
        /// <summary>
        ///   Method which verifies whether the command can execute
        /// </summary>
        private readonly Predicate<object> canExecute;

        /// <summary>
        ///   Method to execute
        /// </summary>
        private readonly Action<object> execute;

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        ///   Fires when the CanExecute status of this command changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        /// <summary>
        ///   Check if the method can be executed
        /// </summary>
        /// <param name = "parameter">Parameter</param>
        /// <returns>True/False</returns>
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute.Invoke(parameter);
        }

        /// <summary>
        ///   Execute the method
        /// </summary>
        /// <param name = "parameter">Parameter for execution</param>
        public void Execute(object parameter)
        {
            execute(parameter);
        }
    }
}