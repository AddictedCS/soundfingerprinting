namespace SoundFingerprinting.DuplicatesDetector
{
    using System.Windows;

    using SoundFingerprinting.DuplicatesDetector.Services;
    using SoundFingerprinting.DuplicatesDetector.ViewModel;

    /// <summary>
    ///   Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow window = new MainWindow(); /*Instantiate the main window*/
            ServiceInjector.InjectServices(); /*Inject Services into the application*/
            ViewModelBase mainViewModel = new MainWindowViewModel();
            window.DataContext = mainViewModel;
            window.Show();
        }
    }
}