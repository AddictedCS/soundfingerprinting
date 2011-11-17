// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;

namespace Soundfingerprinting.DuplicatesDetector.Services
{
    /// <summary>
    ///   Window service works as mediator between the View and View/Model
    /// </summary>
    public interface IWindowService
    {
        void ShowDialog<TViewModel>(IGenericViewWindow view, TViewModel viewModel, Action<object, EventArgs> onDialogClose);

        void ShowDialog<TDialogViewModel>(IGenericViewWindow view, TDialogViewModel viewModel);
    }
}