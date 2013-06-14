namespace SoundFingerprinting.DuplicatesDetector.Services
{
    using System.Windows.Forms;

    internal interface ISaveFileDialogService
    {
        string Filename { get; }

        DialogResult SaveFile(string title, string filename, string extension);
    }
}