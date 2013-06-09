namespace Soundfingerprinting.Infrastructure
{
    using System;
    using System.Collections.Generic;

    using Ninject;

    using Soundfingerprinting.Audio;
    using Soundfingerprinting.Audio.Bass;
    using Soundfingerprinting.Audio.NAudio;
    using Soundfingerprinting.Configuration;
    using Soundfingerprinting.Dao;
    using Soundfingerprinting.FFT;
    using Soundfingerprinting.FFT.FFTW;
    using Soundfingerprinting.Hashing;
    using Soundfingerprinting.Hashing.LSH;
    using Soundfingerprinting.Hashing.MinHash;
    using Soundfingerprinting.Image;
    using Soundfingerprinting.Utils;
    using Soundfingerprinting.Wavelets;

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
                kernel.Bind<ITagService>().To<BassAudioService>().InSingletonScope();
                kernel.Bind<IAudioService, IExtendedAudioService>().To<NAudioService>().InSingletonScope();
                kernel.Bind<IFFTService>().To<CachedFFTWService>();

                if (Environment.Is64BitProcess)
                {
                    kernel.Bind<FFTWService>().To<FFTWService64>().WhenInjectedInto<CachedFFTWService>();
                }
                else
                {
                    kernel.Bind<FFTWService>().To<FFTWService86>().WhenInjectedInto<CachedFFTWService>();
                }

                kernel.Bind<IFingerprintUnitBuilder>().To<FingerprintUnitBuilder>();
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
                kernel.Bind<IPermutations>().To<DefaultPermutations>();
                
                kernel.Bind<ICombinedHashingAlgoritm>().To<CombinedHashingAlgorithm>();
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

            public void Bind<TInterface, TImplementation>() where TImplementation : TInterface
            {
                kernel.Rebind<TInterface>().To<TImplementation>();
            }

            public void Bind<TInterface, TImplementation>(TImplementation constant) where TImplementation : TInterface
            {
                if (constant as IPermutations != null)
                {
                    RemoveBindingsForType(typeof(IPermutations));
                    kernel.Bind<IPermutations>().To<CachedPermutations>();
                    kernel.Bind<IPermutations>().ToConstant((IPermutations)constant).WhenInjectedInto<CachedPermutations>();
                }
                else if (constant as IModelBinderFactory != null)
                {
                    RemoveBindingsForType(typeof(IModelBinderFactory));
                    kernel.Bind<IModelBinderFactory>().To<CachedModelBinderFactory>();
                    kernel.Rebind<IModelBinderFactory>().ToConstant((IModelBinderFactory)constant).WhenInjectedInto<CachedModelBinderFactory>();
                }
                else
                {
                    kernel.Rebind<TInterface>().ToConstant(constant);
                }
            }

            private void RemoveBindingsForType(Type type)
            {
                foreach (var binding in kernel.GetBindings(type))
                {
                    kernel.RemoveBinding(binding);
                }
            }
        }
    }
}
