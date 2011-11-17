// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Windows;

namespace Soundfingerprinting.DuplicatesDetector.Services
{
    /// <summary>
    ///   Service Contract to be implemented by the types which would like to provide MessageBox.Show services
    /// </summary>
    public interface IMessageBoxService
    {
        /// <summary>
        ///   Show the MessageBox to the client
        /// </summary>
        /// <param name = "message">Message to be shown</param>
        /// <param name = "title">Title of the MessageBox</param>
        /// <param name = "buttons">Buttons</param>
        /// <param name = "image">Image to be shown</param>
        /// <returns>MessageBox results</returns>
        MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image);
    }
}