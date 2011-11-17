// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Collections.ObjectModel;

namespace Soundfingerprinting.DuplicatesDetector.ViewModel
{
    public class GenericViewModel : ViewModelBase
    {
        private ObservableCollection<ViewModelBase> _workspaces;

        public ObservableCollection<ViewModelBase> Workspaces
        {
            get { return _workspaces ?? (_workspaces = new ObservableCollection<ViewModelBase>()); }
            private set { _workspaces = value; }
        }
    }
}