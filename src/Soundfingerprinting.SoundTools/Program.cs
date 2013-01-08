namespace Soundfingerprinting.SoundTools
{
    using System;
    using System.Windows.Forms;

    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.Wavelets;
    using Soundfingerprinting.Fingerprinting.Windows;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;
    using Soundfingerprinting.SoundTools.DI;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            IDependencyResolver dependencyResolver = new NinjectDependencyResolver();
            dependencyResolver.Bind<IDependencyResolver>().ToConstant(dependencyResolver);
            dependencyResolver.Bind<IFingerprintService>().To<FingerprintService>();
            dependencyResolver.Bind<IWindowFunction>().To<HanningWindow>();
            dependencyResolver.Bind<IWaveletDecomposition>().To<HaarWavelet>();
            dependencyResolver.Bind<IFingerprintDescriptor>().To<FingerprintDescriptor>();
            dependencyResolver.Bind<IFingerprintingConfiguration>().To<DefaultFingerprintingConfiguration>();
            dependencyResolver.Bind<IAudioService>().To<BassAudioService>();
            dependencyResolver.Bind<ITagService>().To<TagService>();
            dependencyResolver.Bind<IWorkUnitBuilder>().To<WorkUnitBuilder>();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(dependencyResolver.Get<WinMain>());
        }
    }
}