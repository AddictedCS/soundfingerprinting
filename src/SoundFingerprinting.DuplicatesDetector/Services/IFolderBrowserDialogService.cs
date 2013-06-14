namespace SoundFingerprinting.DuplicatesDetector.Services
{
    using System.Windows.Forms;

    /// <summary>
    ///   Contract for FolderBrowserDialog service
    /// </summary>
    public interface IFolderBrowserDialogService
    {
        /// <summary>
        ///   Gets selected path
        /// </summary>
        string SelectedPath { get; }

        /// <summary>
        ///   Show FolderBrowserDialog
        /// </summary>
        /// <returns>Dialog results</returns>
        DialogResult Show();
    }
}