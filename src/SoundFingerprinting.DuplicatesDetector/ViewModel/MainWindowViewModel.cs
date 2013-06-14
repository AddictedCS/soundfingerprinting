namespace SoundFingerprinting.DuplicatesDetector.ViewModel
{
    using System.Collections.ObjectModel;

    /// <summary>
    ///   Main window view model
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<ViewModelBase> workspaces;

        public MainWindowViewModel()
        {
            /*Adding PathList view model to workspaces collection*/
            ViewModelBase pathList = new PathListViewModel();
            Workspaces.Add(pathList);
        }

        public ObservableCollection<ViewModelBase> Workspaces
        {
            get { return workspaces ?? (workspaces = new ObservableCollection<ViewModelBase>()); }
        }
    }
}