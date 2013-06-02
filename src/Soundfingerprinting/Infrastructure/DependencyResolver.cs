namespace Soundfingerprinting.Infrastructure
{
    using System;
    using System.Collections.Generic;

    using Ninject;

    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Dao;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.FFT;
    using Soundfingerprinting.Fingerprinting.FFT.FFTW;
    using Soundfingerprinting.Fingerprinting.FingerprintUnitBuilder;
    using Soundfingerprinting.Fingerprinting.Wavelets;
    using Soundfingerprinting.Hashing;
    using Soundfingerprinting.Hashing.LSH;
    using Soundfingerprinting.Hashing.MinHash;
    using Soundfingerprinting.Image;
    using Soundfingerprinting.Query;

    public static class DependencyResolver
    {
        private static IDependencyResolver dependencyResolver = new DefaultDependencyResolver();

        public static IDependencyResolver Current
        {
            get
            {
                return dependencyResolver;
            }

            set
            {
                dependencyResolver = value;
            }
        }

        private class DefaultDependencyResolver : IDependencyResolver
        {
            private readonly IKernel kernel;

            public DefaultDependencyResolver()
            {
                kernel = new StandardKernel();
                kernel.Bind<IFingerprintService>().To<FingerprintService>();
                kernel.Bind<IWaveletDecomposition>().To<StandardHaarWaveletDecomposition>();
                kernel.Bind<IFingerprintDescriptor>().To<FingerprintDescriptor>();
                kernel.Bind<IFingerprintingConfiguration>().To<DefaultFingerprintingConfiguration>();
                kernel.Bind<IAudioService, IExtendedAudioService, ITagService>().To<BassAudioService>();
                kernel.Bind<IFFTService>().To<CachedFFTWService>();
                kernel.Bind<IFingerprintingUnitsBuilder>().To<FingerprintingUnitsBuilder>();
                kernel.Bind<IDatabaseProviderFactory>().To<MsSqlDatabaseProviderFactory>();
                kernel.Bind<IConnectionStringFactory>().To<DefaultConnectionStringFactory>();
                kernel.Bind<IModelBinderFactory>().To<CachedModelBinderFactory>();
                kernel.Bind<IModelBinderFactory>().To<ModelBinderFactory>().WhenInjectedInto<CachedModelBinderFactory>();
                kernel.Bind<IModelService>().To<ModelService>();
                kernel.Bind<IPermutationGeneratorService>().To<PermutationGeneratorService>();
                kernel.Bind<IImageService>().To<ImageService>();
                kernel.Bind<ISpectrumService>().To<SpectrumService>();
                kernel.Bind<IWaveletService>().To<WaveletService>();
                kernel.Bind<IMinHashService>().To<MinHashService>();
                kernel.Bind<ILSHService>().To<LSHService>();
                kernel.Bind<ICombinedHashingAlgoritm>().To<CombinedHashingAlgorithm>();
                kernel.Bind<IPermutations>()
                                  .To<DbPermutations>()
                                  .WithConstructorArgument("connectionString", kernel.Get<IConnectionStringFactory>().GetConnectionString());
                kernel.Bind<IFingerprintQueryBuilder>().To<FingerprintQueryBuilder>();
                kernel.Bind<IQueryFingerprintService>().To<QueryFingerprintService>();
            }

            public object GetService(Type serviceType)
            {
                return kernel.GetService(serviceType);
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                return kernel.GetAll(serviceType);
            }

            public T Get<T>()
            {
                return kernel.Get<T>();
            }
        }
    }
}
