namespace SoundFingerprinting.DuplicatesDetector.Services
{
    using System.Windows.Forms;

    public interface IOpenFileDialogService
    {
        string[] SelectedPaths { get; }

        DialogResult Show(string title, string filename, string filter, bool multiselect);
    }
}