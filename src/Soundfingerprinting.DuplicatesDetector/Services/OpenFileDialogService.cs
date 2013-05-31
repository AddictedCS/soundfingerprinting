namespace Soundfingerprinting.DuplicatesDetector.Services
{
    using System.Windows.Forms;

    /// <summary>
    ///   Open file dialog service
    /// </summary>
    public class OpenFileDialogService : IOpenFileDialogService
    {
        /// <summary>
        ///   Selected file
        /// </summary>
        private string[] _selectedPath;

        #region IOpenFileDialogService Members

        /// <summary>
        ///   Show open file dialog
        /// </summary>
        /// <param name = "title">Title of the dialog</param>
        /// <param name = "filename">Default filename</param>
        /// <param name = "filter">Filter of the file dialog</param>
        /// <param name = "multiselect">Multi-select enabled</param>
        /// <returns>Dialog result</returns>
        public DialogResult Show(string title, string filename, string filter, bool multiselect)
        {
            using (OpenFileDialog ofd = new OpenFileDialog {Title = title, FileName = filename, Filter = filter, Multiselect = multiselect})
            {
                DialogResult result = ofd.ShowDialog();
                _selectedPath = ofd.FileNames;
                return result;
            }
        }

        /// <summary>
        ///   Selected path
        /// </summary>
        public string[] SelectedPaths
        {
            get { return _selectedPath; }
        }

        #endregion
    }
}