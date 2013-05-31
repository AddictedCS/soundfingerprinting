namespace Soundfingerprinting.DuplicatesDetector.ViewModel
{
    using System.Collections.ObjectModel;

    /// <summary>
    ///   Main window view model
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<ViewModelBase> _workspaces;

        /// <summary>
        ///   Parameter less constructor
        /// </summary>
        public MainWindowViewModel()
        {
            /*Adding PathList view model to workspaces collection*/
            ViewModelBase pathList = new PathListViewModel();
            Workspaces.Add(pathList);
        }

        /// <summary>
        ///   Workspaces
        /// </summary>
        public ObservableCollection<ViewModelBase> Workspaces
        {
            get { return _workspaces ?? (_workspaces = new ObservableCollection<ViewModelBase>()); }
            private set { _workspaces = value; }
        }
    }
}