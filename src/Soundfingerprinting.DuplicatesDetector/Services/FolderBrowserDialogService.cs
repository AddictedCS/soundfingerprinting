namespace SoundFingerprinting.DuplicatesDetector.Services
{
    using System.Windows.Forms;

    public class FolderBrowserDialogService : IFolderBrowserDialogService
    {
        #region IFolderBrowserDialogService Members

        public string SelectedPath { get; private set; }

        public DialogResult Show()
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                DialogResult result = dlg.ShowDialog();
                SelectedPath = dlg.SelectedPath;
                return result;
            }
        }

        #endregion
    }
}