namespace SoundFingerprinting.DuplicatesDetector.Services
{
    using System;
    using System.ComponentModel;

    using SoundFingerprinting.DuplicatesDetector.View;

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
        private GenericView view;

        #region IGenericViewWindow Members

        public event EventHandler Closed;

        public event CancelEventHandler Closing;

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
        
        public void Show()
        {
            GetView().Show();
        }

        public void Close()
        {
            GetView().Close();
            view = null;
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
                if (view != null)
                {
                    return view;
                }

                view = new GenericView();
                view.Closed += (sender, e) =>
                    {
                        if (Closed != null)
                        {
                            Closed(sender, e);
                        }

                        view = null;
                    };
                view.Closing += (sender, e) =>
                    {
                        if (Closing != null)
                        {
                            Closing(sender, e);
                        }
                    };
                return view;
            }
        }
    }
}