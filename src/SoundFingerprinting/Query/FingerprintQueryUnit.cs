namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Hashing.MinHash;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Query.Configuration;

    internal sealed class FingerprintingQueryUnit : IQuerySource, IWithQueryConfiguration, IWithQueryAndFingerprintConfiguration, IFingerprintQueryUnit
    {
        private readonly IFingerprintUnitBuilder fingerprintUnitBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        
        private Func<IWithAlgorithmConfiguration> fingerprintingMethodFromSelector;
        private Func<IAudioFingerprintingUnit> createFingerprintMethod;
        private IQueryConfiguration queryConfiguration;

        public FingerprintingQueryUnit(IFingerprintUnitBuilder fingerprintUnitBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintUnitBuilder = fingerprintUnitBuilder;
            this.queryFingerprintService = queryFingerprintService;
        }

        public IWithQueryAndFingerprintConfiguration From(string pathToAudioFile)
        {
            fingerprintingMethodFromSelector = () => fingerprintUnitBuilder.BuildAudioFingerprintingUnit().From(pathToAudioFile);
            return this;
        }

        public IWithQueryAndFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond)
        {
            fingerprintingMethodFromSelector = () => fingerprintUnitBuilder.BuildAudioFingerprintingUnit().From(pathToAudioFile, secondsToProcess, startAtSecond);
            return this;
        }

        public IWithQueryAndFingerprintConfiguration From(float[] audioSamples)
        {
            fingerprintingMethodFromSelector = () => fingerprintUnitBuilder.BuildAudioFingerprintingUnit().From(audioSamples);
            return this;
        }

        public IWithQueryConfiguration From(bool[] fingerprint)
        {
            fingerprintingMethodFromSelector = () => new EmptyWithAlgorithmConfiguration(fingerprint);
            return this;
        }

        public IFingerprintQueryUnit WithConfigurations(IFingerprintingConfiguration fingerprintingConfiguration, IQueryConfiguration configuration)
        {
            queryConfiguration = configuration;
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithAlgorithmConfiguration(fingerprintingConfiguration);
            return this;
        }

        public IFingerprintQueryUnit WithConfigurations<T1, T2>() where T1 : IFingerprintingConfiguration, new() where T2 : IQueryConfiguration, new()
        {
            queryConfiguration = new T2();
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithAlgorithmConfiguration<T1>();
            return this;
        }

        public IFingerprintQueryUnit WithCustomConfigurations(
            Action<CustomFingerprintingConfiguration> fingerprintingConfigurationTransformation, Action<CustomQueryConfiguration> queryConfigurationTransformation)
        {
            CustomQueryConfiguration customQueryConfiguration = new CustomQueryConfiguration();
            queryConfiguration = customQueryConfiguration;
            queryConfigurationTransformation(customQueryConfiguration);
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithCustomAlgorithmConfiguration(fingerprintingConfigurationTransformation);
            return this;
        }

        public IFingerprintQueryUnit WithDefaultConfigurations()
        {
            queryConfiguration = new DefaultQueryConfiguration();
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithDefaultAlgorithmConfiguration();
            return this;
        }

        public Task<QueryResult> Query()
        {
            return createFingerprintMethod().FingerprintIt().AsIs()
                                            .ContinueWith(task => queryFingerprintService.Query(task.Result, queryConfiguration));
        }

        public Task<QueryResult> Query(CancellationToken cancelationToken)
        {
            return createFingerprintMethod().FingerprintIt().AsIs(cancelationToken)
                                            .ContinueWith(task =>
                                            {
                                                if (cancelationToken.IsCancellationRequested)
                                                {
                                                    return new QueryResult { IsSuccessful = false };
                                                }

                                                return queryFingerprintService.Query(task.Result, queryConfiguration);
                                            });
        }

        public IFingerprintQueryUnit WithQueryConfiguration(IQueryConfiguration configuration)
        {
            queryConfiguration = configuration;
            return this;
        }

        private class EmptyWithAlgorithmConfiguration : IWithAlgorithmConfiguration
        {
            private readonly bool[] fingerprint;

            private readonly IMinHashService minHashService;

            public EmptyWithAlgorithmConfiguration(IMinHashService minHashService)
            {
                this.minHashService = minHashService;
            }

            public EmptyWithAlgorithmConfiguration(bool[] fingerprint)
                : this(DependencyResolver.Current.Get<IMinHashService>())
            {
                this.fingerprint = fingerprint;
            }

            public IAudioFingerprintingUnit WithAlgorithmConfiguration(IFingerprintingConfiguration configuration)
            {
                return new EmptyAudioFingerprintingUnit(fingerprint, minHashService);
            }

            public IAudioFingerprintingUnit WithAlgorithmConfiguration<T>() where T : IFingerprintingConfiguration, new()
            {
                return new EmptyAudioFingerprintingUnit(fingerprint, minHashService);
            }

            public IAudioFingerprintingUnit WithCustomAlgorithmConfiguration(Action<CustomFingerprintingConfiguration> functor)
            {
                return new EmptyAudioFingerprintingUnit(fingerprint, minHashService);
            }

            public IAudioFingerprintingUnit WithDefaultAlgorithmConfiguration()
            {
                return new EmptyAudioFingerprintingUnit(fingerprint, minHashService);
            }

            private class EmptyAudioFingerprintingUnit : IAudioFingerprintingUnit, IFingerprinter, IHasher
            {
                private readonly bool[] fingerprint;

                private readonly IMinHashService minHashService;

                public EmptyAudioFingerprintingUnit(bool[] fingerprint, IMinHashService minHashService)
                {
                    this.fingerprint = fingerprint;
                    this.minHashService = minHashService;
                }

                public IFingerprintingConfiguration Configuration { get; set; }

                public IFingerprinter FingerprintIt()
                {
                    return this;
                }

                Task<List<byte[]>> IHasher.AsIs()
                {
                    TaskCompletionSource<List<byte[]>> tcs = new TaskCompletionSource<List<byte[]>>();
                    tcs.SetResult(new List<byte[]> { minHashService.Hash(fingerprint) });
                    return tcs.Task;
                }

                Task<List<byte[]>> IHasher.AsIs(CancellationToken token)
                {
                    return ((IHasher)this).AsIs();
                }

                Task<List<SubFingerprint>> IHasher.ForTrack(int trackId)
                {
                    TaskCompletionSource<List<SubFingerprint>> tcs = new TaskCompletionSource<List<SubFingerprint>>();
                    tcs.SetResult(new List<SubFingerprint> { new SubFingerprint(minHashService.Hash(fingerprint), trackId) });
                    return tcs.Task;
                }

                Task<List<SubFingerprint>> IHasher.ForTrack(int trackId, CancellationToken token)
                {
                    return ((IHasher)this).ForTrack(trackId);
                }

                public IHasher HashIt()
                {
                    return this;
                }

                Task<List<bool[]>> IFingerprinter.AsIs()
                {
                    TaskCompletionSource<List<bool[]>> tcs = new TaskCompletionSource<List<bool[]>>();
                    tcs.SetResult(new List<bool[]> { fingerprint });
                    return tcs.Task;
                }

                Task<List<bool[]>> IFingerprinter.AsIs(CancellationToken token)
                {
                    return ((IFingerprinter)this).AsIs();
                }

                Task<List<Fingerprint>> IFingerprinter.ForTrack(int trackId)
                {
                    TaskCompletionSource<List<Fingerprint>> tcs = new TaskCompletionSource<List<Fingerprint>>();
                    tcs.SetResult(new List<Fingerprint> { new Fingerprint(fingerprint, trackId) });
                    return tcs.Task;
                }

                Task<List<Fingerprint>> IFingerprinter.ForTrack(int trackId, CancellationToken token)
                {
                    return ((IFingerprinter)this).ForTrack(trackId);
                }
            }
        }
    }
}