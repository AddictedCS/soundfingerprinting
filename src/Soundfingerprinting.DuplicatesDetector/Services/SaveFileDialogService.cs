namespace Soundfingerprinting.DuplicatesDetector.Services
{
    using System.Windows.Forms;

    internal class SaveFileDialogService : ISaveFileDialogService
    {
        #region ISaveFileDialogService Members

        public string Filename { get; private set; }

        public DialogResult SaveFile(string title, string filename, string extension)
        {
            using (SaveFileDialog sfd = new SaveFileDialog { Title = title, FileName = filename, Filter = extension })
            {
                DialogResult result = sfd.ShowDialog();
                Filename = sfd.FileName;
                return result;
            }
        }

        #endregion
    }
}