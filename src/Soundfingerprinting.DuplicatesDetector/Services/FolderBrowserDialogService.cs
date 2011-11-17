// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Windows.Forms;

namespace Soundfingerprinting.DuplicatesDetector.Services
{
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