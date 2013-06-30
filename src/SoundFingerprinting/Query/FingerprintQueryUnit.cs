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
    using SoundFingerprinting.Query.Configuration;

    internal sealed class FingerprintingQueryUnit : IQuerySource, IWithQueryConfiguration, IWithQueryAndFingerprintConfiguration, IFingerprintQueryUnit
    {
        private readonly IFingerprintUnitBuilder fingerprintUnitBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        private readonly IMinHashService minHashService;

        private Func<IWithAlgorithmConfiguration> fingerprintingMethodFromSelector;
        private Func<IFingerprintUnit> createFingerprintMethod;
        private IQueryConfiguration queryConfiguration;

        public FingerprintingQueryUnit(IFingerprintUnitBuilder fingerprintUnitBuilder, IQueryFingerprintService queryFingerprintService, IMinHashService minHashService)
        {
            this.fingerprintUnitBuilder = fingerprintUnitBuilder;
            this.queryFingerprintService = queryFingerprintService;
            this.minHashService = minHashService;
        }

        public IWithQueryAndFingerprintConfiguration From(string pathToAudioFile)
        {
            fingerprintingMethodFromSelector = () => fingerprintUnitBuilder.BuildFingerprints().From(pathToAudioFile);
            return this;
        }

        public IWithQueryAndFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond)
        {
            fingerprintingMethodFromSelector = () => fingerprintUnitBuilder.BuildFingerprints().From(pathToAudioFile, secondsToProcess, startAtSecond);
            return this;
        }

        public IWithQueryAndFingerprintConfiguration From(float[] audioSamples)
        {
            fingerprintingMethodFromSelector = () => fingerprintUnitBuilder.BuildFingerprints().From(audioSamples);
            return this;
        }

        public IWithQueryConfiguration From(bool[] fingerprint)
        {
            fingerprintingMethodFromSelector = () => new EmptyWithAlgorithmConfiguration(fingerprint, minHashService);
            return this;
        }

        public IFingerprintQueryUnit With(IFingerprintingConfiguration fingerprintingConfiguration, IQueryConfiguration configuration)
        {
            queryConfiguration = configuration;
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithAlgorithmConfiguration(fingerprintingConfiguration);
            return this;
        }

        public IFingerprintQueryUnit With<T1, T2>() where T1 : IFingerprintingConfiguration, new() where T2 : IQueryConfiguration, new()
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

        public IFingerprintQueryUnit With(IQueryConfiguration configuration)
        {
            queryConfiguration = configuration;
            return this;
        }

        private class EmptyWithAlgorithmConfiguration : IWithAlgorithmConfiguration
        {
            private readonly bool[] fingerprint;

            private readonly IMinHashService minHashService;

            public EmptyWithAlgorithmConfiguration(bool[] fingerprint, IMinHashService minHashService)
            {
                this.fingerprint = fingerprint;
                this.minHashService = minHashService;
            }

            public IFingerprintUnit WithAlgorithmConfiguration(IFingerprintingConfiguration configuration)
            {
                return new EmptyFingerprintUnit(fingerprint, minHashService);
            }

            public IFingerprintUnit WithAlgorithmConfiguration<T>() where T : IFingerprintingConfiguration, new()
            {
                return new EmptyFingerprintUnit(fingerprint, minHashService);
            }

            public IFingerprintUnit WithCustomAlgorithmConfiguration(Action<CustomFingerprintingConfiguration> functor)
            {
                return new EmptyFingerprintUnit(fingerprint, minHashService);
            }

            public IFingerprintUnit WithDefaultAlgorithmConfiguration()
            {
                return new EmptyFingerprintUnit(fingerprint, minHashService);
            }

            private class EmptyFingerprintUnit : IFingerprintUnit, IFingerprinter, IHasher
            {
                private readonly bool[] fingerprint;

                private readonly IMinHashService minHashService;

                public EmptyFingerprintUnit(bool[] fingerprint, IMinHashService minHashService)
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