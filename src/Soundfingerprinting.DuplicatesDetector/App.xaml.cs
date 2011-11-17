// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Windows;
using Soundfingerprinting.DuplicatesDetector.Services;
using Soundfingerprinting.DuplicatesDetector.ViewModel;

namespace Soundfingerprinting.DuplicatesDetector
{
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