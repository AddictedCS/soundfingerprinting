namespace Soundfingerprinting.DuplicatesDetector.Services
{
    using System.Windows.Forms;

    internal class SaveFileDialogService : ISaveFileDialogService
    {
        private string _filename;

        #region ISaveFileDialogService Members

        public DialogResult SaveFile(string title, string filename, string extension)
        {
            using (SaveFileDialog sfd = new SaveFileDialog {Title = title, FileName = filename, Filter = extension})
            {
                DialogResult result = sfd.ShowDialog();
                _filename = sfd.FileName;
                return result;
            }
        }

        public string Filename
        {
            get { return _filename; }
        }

        #endregion
    }
}