// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.ComponentModel;

namespace Soundfingerprinting.DuplicatesDetector.Services
{
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