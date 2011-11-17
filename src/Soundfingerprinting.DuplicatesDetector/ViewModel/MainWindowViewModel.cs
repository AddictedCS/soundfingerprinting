// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Collections.ObjectModel;

namespace Soundfingerprinting.DuplicatesDetector.ViewModel
{
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