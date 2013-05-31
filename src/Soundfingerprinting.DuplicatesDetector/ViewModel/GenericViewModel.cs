namespace Soundfingerprinting.DuplicatesDetector.ViewModel
{
    using System.Collections.ObjectModel;

    public class GenericViewModel : ViewModelBase
    {
        private ObservableCollection<ViewModelBase> workspaces;

        public ObservableCollection<ViewModelBase> Workspaces
        {
            get { return workspaces ?? (workspaces = new ObservableCollection<ViewModelBase>()); }
        }
    }
}