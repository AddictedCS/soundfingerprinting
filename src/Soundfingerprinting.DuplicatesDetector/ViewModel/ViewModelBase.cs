namespace Soundfingerprinting.DuplicatesDetector.ViewModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    using Ninject;

    using Soundfingerprinting.DuplicatesDetector.Services;

    /// <summary>
    ///   Base class for ViewModels
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        private bool alreadyDisposed;

        ~ViewModelBase()
        {
            Dispose(false);
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        ///   Property changed event
        /// </summary>
        /// <remarks>
        ///   Whenever a property on a ViewModel object has a new value, 
        ///   it can raise the PropertyChanged event to notify the WPF binding 
        ///   system of the new value. Upon receiving that notification, the 
        ///   binding system queries the property, and the bound property on 
        ///   some UI element receives the new value.
        /// </remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        ///   Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;
                throw new Exception(msg);
            }
        }

        /// <summary>
        ///   Get service contract implementation of a specific type
        /// </summary>
        /// <typeparam name = "TServiceContract">Type of the service contract</typeparam>
        /// <returns>Implementation of service contract</returns>
        public TServiceContract GetService<TServiceContract>()
        {
            return ServiceContainer.Kernel.Get<TServiceContract>();
        }

        /// <summary>
        ///   On property changed, tell UI to refresh binding elements
        /// </summary>
        /// <param name = "propertyName">Changed property</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);
            PropertyChangedEventHandler temp = PropertyChanged;
            if (temp != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        ///   Dispose
        /// </summary>
        /// <param name = "isDisposing">Is disposing by client call</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (!alreadyDisposed)
            {
                if (!isDisposing)
                {
                    /*Dispose managed resources*/
                }

                /*Dispose unmanaged resources*/
            }

            alreadyDisposed = true;
        }
    }
}