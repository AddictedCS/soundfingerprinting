namespace Soundfingerprinting.DuplicatesDetector.Services
{
    using System.Windows.Forms;

    public interface IOpenFileDialogService
    {
        /// <summary>
        ///   Gets selected path
        /// </summary>
        string[] SelectedPaths { get; }

        /// <summary>
        ///   Show FolderBrowserDialog
        /// </summary>
        /// <returns>Dialog results</returns>
        DialogResult Show(string title, string filename, string filter, bool multiselect);
    }
}