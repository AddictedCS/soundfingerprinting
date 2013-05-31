namespace Soundfingerprinting.DuplicatesDetector.Services
{
    using System;
    using System.ComponentModel;

    /// <summary>
    ///   Interface to be implemented by views
    /// </summary>
    /// <remarks>
    ///   The binding between the views and view-models will be performed by a 
    ///   mediator IWindowService which will take care of the abstraction
    /// </remarks>
    public interface IGenericViewWindow
    {
        bool? DialogResult { get; set; }
        object DataContext { get; set; }
        event EventHandler Closed;
        event CancelEventHandler Closing;
        void Show();
        void Close();
    }
}