namespace SoundFingerprinting.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ninject;
    using Ninject.Parameters;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.RAM;
    using SoundFingerprinting.Dao.SQL;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.FFT.FFTW;
    using SoundFingerprinting.Hashing;
    using SoundFingerprinting.Hashing.MinHash;
    using SoundFingerprinting.Image;
    using SoundFingerprinting.Utils;
    using SoundFingerprinting.Wavelets;

    public static class DependencyResolver
    {
        private static IDependencyResolver dependencyResolver;

        public static IDependencyResolver Current
        {
            get
            {
                return dependencyResolver ?? (dependencyResolver = new DefaultDependencyResolver());
            }

            set
            {
                dependencyResolver = value;
            }
        }

        private sealed class DefaultDependencyResolver : IDependencyResolver, IDisposable
        {
            private readonly IKernel kernel;

            public DefaultDependencyResolver()
            {
                kernel = new StandardKernel();
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
                kernel.Bind<ITagService, IAudioService, IExtendedAudioService>().To<BassAudioService>().InSingletonScope();
              
                kernel.Bind<IModelBinderFactory>().To<CachedModelBinderFactory>().InSingletonScope();
                kernel.Bind<IModelBinderFactory>().To<ModelBinderFactory>()
                                                  .WhenInjectedInto<CachedModelBinderFactory>();
              
                kernel.Bind<IDatabaseProviderFactory>().To<MsSqlDatabaseProviderFactory>();
                kernel.Bind<IConnectionStringFactory>().To<DefaultConnectionStringFactory>().InSingletonScope();
                
                kernel.Bind<IModelService>().To<SqlModelService>().InSingletonScope();
                kernel.Bind<IImageService>().To<ImageService>();
                kernel.Bind<IMinHashService>().To<MinHashService>().InSingletonScope();
                kernel.Bind<IPermutations>().To<DefaultPermutations>().InSingletonScope();
                kernel.Bind<ILocalitySensitiveHashingAlgorithm>().To<LocalitySensitiveHashingAlgorithm>().InSingletonScope();

                kernel.Bind<IFingerprintCommandBuilder>().To<FingerprintCommandBuilder>();
                kernel.Bind<IQueryFingerprintService>().To<QueryFingerprintService>();
                kernel.Bind<IQueryCommandBuilder>().To<QueryCommandBuilder>();
                
                kernel.Bind<IRAMStorage>().To<RAMStorage>()
                                          .InSingletonScope()
                                          .WithConstructorArgument("numberOfHashTables", new DefaultFingerprintConfiguration().NumberOfLSHTables);
            }

            ~DefaultDependencyResolver()
            {
                Dispose(false);
            }

            public T Get<T>()
            {
                return kernel.Get<T>();
            }

            public TInterface Get<TInterface>(IEnumerable<ConstructorArgument> constructorArguments)
            {
                var arguments = constructorArguments.Select(constructorArgument => (IParameter)new Ninject.Parameters.ConstructorArgument(constructorArgument.Name, constructorArgument.Instance)).ToList();
                return kernel.Get<TInterface>(arguments.ToArray());
            }

            public void Bind<TInterface, TImplementation>() where TImplementation : TInterface
            {
                kernel.Rebind<TInterface>().To<TImplementation>();
            }

            public void BindAsSingleton<TInterface, TImplementation>() where TImplementation : TInterface
            {
                kernel.Rebind<TInterface>().To<TImplementation>().InSingletonScope();
            }

            public void Bind<TInterface, TImplementation>(TImplementation constant) where TImplementation : TInterface
            {
                kernel.Rebind<TInterface>().ToConstant(constant);
            }

            public void BindAsSingleton<TInterface, TImplementation>(TImplementation constant) where TImplementation : TInterface
            {
                kernel.Rebind<TInterface>().To<TImplementation>().InSingletonScope();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool isDisposing)
            {
                if (isDisposing)
                {
                    kernel.Dispose();
                }
            }
        }
    }
}
