namespace SoundFingerprinting.DuplicatesDetector.Services
{
    using System;

    /// <summary>
    ///   Window service works as mediator between the View and View/Model
    /// </summary>
    public interface IWindowService
    {
        void ShowDialog<TViewModel>(IGenericViewWindow view, TViewModel viewModel, Action<object, EventArgs> onDialogClose);

        void ShowDialog<TDialogViewModel>(IGenericViewWindow view, TDialogViewModel viewModel);
    }
}