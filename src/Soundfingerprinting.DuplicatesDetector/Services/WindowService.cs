namespace Soundfingerprinting.DuplicatesDetector.Services
{
    using System;

    public class WindowService : IWindowService
    {
        #region IWindowService Members

        public void ShowDialog<TViewModel>(IGenericViewWindow view, TViewModel viewModel, Action<object, EventArgs> onDialogClose)
        {
            view.DataContext = viewModel;
            if (onDialogClose != null)
            {
                view.Closing += (o, args) => onDialogClose(o, args);
            }

            view.Show();
        }

        public void ShowDialog<TViewModel>(IGenericViewWindow view, TViewModel viewModel)
        {
            ShowDialog(view, viewModel, null);
        }

        #endregion
    }
}