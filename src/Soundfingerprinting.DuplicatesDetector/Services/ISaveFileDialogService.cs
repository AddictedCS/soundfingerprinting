// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Windows.Forms;

namespace Soundfingerprinting.DuplicatesDetector.Services
{
    internal interface ISaveFileDialogService
    {
        string Filename { get; }
        DialogResult SaveFile(string title, string filename, string extension);
    }
}