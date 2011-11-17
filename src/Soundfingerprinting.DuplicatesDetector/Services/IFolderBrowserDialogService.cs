// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Windows.Forms;

namespace Soundfingerprinting.DuplicatesDetector.Services
{
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