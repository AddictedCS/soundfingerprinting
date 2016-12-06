namespace SoundFingerprinting.Infrastructure
{
    using Ninject;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
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
            kernel.Bind<IFingerprintService>().To<FingerprintService>().InSingletonScope();
            kernel.Bind<ISpectrumService>().To<SpectrumService>().InSingletonScope();
            kernel.Bind<ILogUtility>().To<LogUtility>().InSingletonScope();
            kernel.Bind<IAudioSamplesNormalizer>().To<AudioSamplesNormalizer>().InSingletonScope();
            kernel.Bind<IWaveletDecomposition>().To<StandardHaarWaveletDecomposition>().InSingletonScope();
            kernel.Bind<IFFTService>().To<LomontFFT>().InSingletonScope();
            kernel.Bind<IFingerprintDescriptor>().To<FastFingerprintDescriptor>().InSingletonScope();
            kernel.Bind<IMinHashService>().To<MinHashService>().InSingletonScope();
            kernel.Bind<IPermutations>().To<DefaultPermutations>().InSingletonScope();
            kernel.Bind<ILocalitySensitiveHashingAlgorithm>().To<LocalitySensitiveHashingAlgorithm>().InSingletonScope();
            kernel.Bind<ISimilarityUtility>().To<SimilarityUtility>().InSingletonScope();
            kernel.Bind<IQueryMath>().To<QueryMath>().InSingletonScope();
            kernel.Bind<IQueryResultCoverageCalculator>().To<QueryResultCoverageCalculator>().InSingletonScope();
            kernel.Bind<IConfidenceCalculator>().To<ConfidenceCalculator>().InSingletonScope();
            kernel.Bind<ITestRunnerUtils>().To<TestRunnerUtils>().InSingletonScope();
            kernel.Bind<IHashConverter>().To<HashConverter>().InSingletonScope();

            kernel.Bind<IFingerprintCommandBuilder>().To<FingerprintCommandBuilder>().InSingletonScope();
            kernel.Bind<IQueryFingerprintService>().To<QueryFingerprintService>().InSingletonScope();
            kernel.Bind<IQueryCommandBuilder>().To<QueryCommandBuilder>().InSingletonScope();

            kernel.Bind<IRAMStorage>().To<RAMStorage>()
                                      .InSingletonScope()
                                      .WithConstructorArgument("numberOfHashTables", new DefaultFingerprintConfiguration().HashingConfig.NumberOfLSHTables);
        }
    }
}
