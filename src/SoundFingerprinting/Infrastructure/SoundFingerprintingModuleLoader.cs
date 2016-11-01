namespace SoundFingerprinting.Infrastructure
{
    using System;

    using Ninject;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.FFT.FFTW;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.LCS;
    using SoundFingerprinting.LSH;
    using SoundFingerprinting.Math;
    using SoundFingerprinting.MinHash;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    internal class SoundFingerprintingModuleLoader : IModuleLoader
    {
        public void LoadAssemblyBindings(IKernel kernel)
        {
            kernel.Bind<IFingerprintService>().To<FingerprintService>();
            kernel.Bind<ISpectrumService>().To<SpectrumService>();
            kernel.Bind<ILogUtility>().To<LogUtility>().InSingletonScope();
            kernel.Bind<IAudioSamplesNormalizer>().To<AudioSamplesNormalizer>().InSingletonScope();
            kernel.Bind<IWaveletDecomposition>().To<StandardHaarWaveletDecomposition>().InSingletonScope();
            kernel.Bind<IFFTService>().To<CachedFFTWService>().InSingletonScope();
            if (Environment.Is64BitProcess)
            {
                kernel.Bind<FFTWService>().To<FFTWService64>().WhenInjectedInto<CachedFFTWService>().InSingletonScope();
            }
            else
            {
                kernel.Bind<FFTWService>().To<FFTWService86>().WhenInjectedInto<CachedFFTWService>().InSingletonScope();
            }

            kernel.Bind<IFingerprintDescriptor>().To<FingerprintDescriptor>().InSingletonScope();
            
            kernel.Bind<IMinHashService>().To<MinHashService>().InSingletonScope();
            kernel.Bind<IPermutations>().To<DefaultPermutations>().InSingletonScope();
            kernel.Bind<ILocalitySensitiveHashingAlgorithm>().To<LocalitySensitiveHashingAlgorithm>().InSingletonScope();
            kernel.Bind<IAudioSequencesAnalyzer>().To<AudioSequencesAnalyzer>().InSingletonScope();
            kernel.Bind<ISimilarityUtility>().To<SimilarityUtility>().InSingletonScope();
            kernel.Bind<IQueryMath>().To<QueryMath>().InSingletonScope();

            kernel.Bind<IFingerprintCommandBuilder>().To<FingerprintCommandBuilder>();
            kernel.Bind<IQueryFingerprintService>().To<QueryFingerprintService>();
            kernel.Bind<IQueryCommandBuilder>().To<QueryCommandBuilder>();

            kernel.Bind<IRAMStorage>().To<RAMStorage>()
                                      .InSingletonScope()
                                      .WithConstructorArgument("numberOfHashTables", new DefaultFingerprintConfiguration().HashingConfig.NumberOfLSHTables);
        }
    }
}
