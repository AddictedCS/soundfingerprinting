// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Windows.Forms;

namespace Soundfingerprinting.DuplicatesDetector.Services
{
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