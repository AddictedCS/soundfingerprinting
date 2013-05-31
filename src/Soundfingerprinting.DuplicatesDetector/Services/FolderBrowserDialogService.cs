namespace Soundfingerprinting.DuplicatesDetector.Services
{
    using System.Windows.Forms;

    /// <summary>
    ///   Service for FolderBrowser dialog used in injection
    /// </summary>
    public class FolderBrowserDialogService : IFolderBrowserDialogService
    {
        /// <summary>
        ///   Selected path
        /// </summary>
        private string _selectedPath;

        #region IFolderBrowserDialogService Members

        /// <summary>
        ///   Show FolderBrowserDialog
        /// </summary>
        /// <returns></returns>
        public DialogResult Show()
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                DialogResult result = dlg.ShowDialog();
                _selectedPath = dlg.SelectedPath;
                return result;
            }
        }

        /// <summary>
        ///   Show selected path
        /// </summary>
        public string SelectedPath
        {
            get { return _selectedPath; }
        }

        #endregion
    }
}