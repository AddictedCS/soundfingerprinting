namespace Soundfingerprinting.DuplicatesDetector.Services
{
    using System.Windows;

    /// <summary>
    ///   MessageBox service
    /// </summary>
    public class MessageBoxService : IMessageBoxService
    {
        #region IMessageBoxService Members

        /// <summary>
        ///   Show actual MessageBox
        /// </summary>
        /// <param name = "message">Message to be shown</param>
        /// <param name = "title">Title of the MessageBox</param>
        /// <param name = "buttons">Buttons in the MessageBox</param>
        /// <param name = "image">Image on the MessageBox</param>
        /// <returns>MessageBox results</returns>
        public MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image)
        {
            return MessageBox.Show(message, title, buttons, image);
        }

        #endregion
    }
}