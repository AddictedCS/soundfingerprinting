namespace Soundfingerprinting.DuplicatesDetector.Services
{
    using System;
    using System.ComponentModel;

    using Soundfingerprinting.DuplicatesDetector.View;

    /// <summary>
    ///   Generic view window service
    /// </summary>
    public class GenericViewWindowService : IGenericViewWindow
    {
        /// <summary>
        ///   Lock object
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        ///   Actual view (uninitialized)
        /// </summary>
        private GenericView _view;

        #region IGenericViewWindow Members

        public bool? DialogResult
        {
            get { return GetView().DialogResult; }
            set { GetView().DialogResult = value; }
        }

        public object DataContext
        {
            get { return GetView().DataContext; }
            set { GetView().DataContext = value; }
        }

        public event EventHandler Closed;
        public event CancelEventHandler Closing;

        public void Show()
        {
            GetView().Show();
        }

        public void Close()
        {
            GetView().Close();
            _view = null;
        }

        #endregion

        /// <summary>
        ///   Lazy initialization of the view
        /// </summary>
        /// <returns>Actual view</returns>
        private GenericView GetView()
        {
            lock (LockObject)
            {
                if (_view != null)
                    return _view;
                _view = new GenericView();
                _view.Closed += (sender, e) =>
                                {
                                    if (Closed != null)
                                        Closed.Invoke(sender, e);
                                    _view = null;
                                };
                _view.Closing += (sender, e) =>
                                 {
                                     if (Closing != null)
                                         Closing.Invoke(sender, e);
                                 };
                return _view;
            }
        }
    }
}