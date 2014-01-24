namespace SoundFingerprinting.DuplicatesDetector
{
    using System.Windows;

    using SoundFingerprinting.DuplicatesDetector.Infrastructure;
    using SoundFingerprinting.DuplicatesDetector.Services;
    using SoundFingerprinting.DuplicatesDetector.ViewModel;

    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow window = new MainWindow();
            ServiceInjector.InjectServices();
            ViewModelBase mainViewModel = new MainWindowViewModel();
            window.DataContext = mainViewModel;
            window.Show();
        }
    }
}