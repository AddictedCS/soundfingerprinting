// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.ComponentModel;
using System.Diagnostics;
using Ninject;
using Soundfingerprinting.DuplicatesDetector.Services;

namespace Soundfingerprinting.DuplicatesDetector.ViewModel
{
    /// <summary>
    ///   Base class for ViewModels
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        ///   Throw on invalid property name
        /// </summary>
        private const bool THROW_ON_INVALID_PROPERTY_NAME = true;

        /// <summary>
        ///   Check if already disposed
        /// </summary>
        private bool _alreadyDisposed;

        #region IDisposable Members

        /// <summary>
        ///   Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

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
            if (!_alreadyDisposed)
            {
                if (!isDisposing)
                {
                    /*Dispose managed resources*/
                }
                /*Dispose unmanaged resources*/
            }
            _alreadyDisposed = true;
        }

        /// <summary>
        ///   Finalizer
        /// </summary>
        ~ViewModelBase()
        {
            Dispose(false);
        }

        /// <summary>
        ///   Verify if there is such a property on the ViewModelBase
        /// </summary>
        /// <param name = "propertyName"></param>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (THROW_ON_INVALID_PROPERTY_NAME)
                {
                    throw new Exception(msg);
                }
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
    }
}