namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Hashing.MinHash;
    using SoundFingerprinting.Query.Configuration;

    internal sealed class FingerprintingQueryUnit : IQuerySource, IWithQueryConfiguration, IWithQueryAndFingerprintConfiguration, IFingerprintQueryUnit
    {
        private readonly IFingerprintUnitBuilder fingerprintUnitBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        private readonly IMinHashService minHashService;

        private Func<IWithFingerprintConfiguration> fingerprintingMethodFromSelector;
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
            fingerprintingMethodFromSelector = () => fingerprintUnitBuilder.BuildFingerprints().On(pathToAudioFile);
            return this;
        }

        public IWithQueryAndFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond)
        {
            fingerprintingMethodFromSelector = () => fingerprintUnitBuilder.BuildFingerprints().On(pathToAudioFile, secondsToProcess, startAtSecond);
            return this;
        }

        public IWithQueryAndFingerprintConfiguration From(float[] audioSamples)
        {
            fingerprintingMethodFromSelector = () => fingerprintUnitBuilder.BuildFingerprints().On(audioSamples);
            return this;
        }

        public IWithQueryConfiguration From(bool[] fingerprint)
        {
            fingerprintingMethodFromSelector = () => new EmptyWithFingerprintConfiguration(fingerprint, minHashService);
            return this;
        }

        public IFingerprintQueryUnit With(IFingerprintingConfiguration fingerprintingConfiguration, IQueryConfiguration configuration)
        {
            queryConfiguration = configuration;
            createFingerprintMethod = () => fingerprintingMethodFromSelector().With(fingerprintingConfiguration);
            return this;
        }

        public IFingerprintQueryUnit With<T1, T2>() where T1 : IFingerprintingConfiguration, new() where T2 : IQueryConfiguration, new()
        {
            queryConfiguration = new T2();
            createFingerprintMethod = () => fingerprintingMethodFromSelector().With<T1>();
            return this;
        }

        public IFingerprintQueryUnit WithCustomConfigurations(
            Action<CustomFingerprintingConfiguration> fingerprintingConfigurationTransformation, Action<CustomQueryConfiguration> queryConfigurationTransformation)
        {
            CustomQueryConfiguration customQueryConfiguration = new CustomQueryConfiguration();
            queryConfiguration = customQueryConfiguration;
            queryConfigurationTransformation(customQueryConfiguration);
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithCustomConfiguration(fingerprintingConfigurationTransformation);
            return this;
        }

        public IFingerprintQueryUnit WithDefaultConfigurations()
        {
            queryConfiguration = new DefaultQueryConfiguration();
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithDefaultConfiguration();
            return this;
        }

        public Task<QueryResult> Query()
        {
            return createFingerprintMethod().RunAlgorithm()
                                            .ContinueWith(task => queryFingerprintService.Query(task.Result, queryConfiguration));
        }

        public Task<QueryResult> Query(CancellationToken cancelationToken)
        {
            return createFingerprintMethod().RunAlgorithm(cancelationToken)
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

        private class EmptyWithFingerprintConfiguration : IWithFingerprintConfiguration
        {
            private readonly bool[] fingerprint;

            private readonly IMinHashService minHashService;

            public EmptyWithFingerprintConfiguration(bool[] fingerprint, IMinHashService minHashService)
            {
                this.fingerprint = fingerprint;
                this.minHashService = minHashService;
            }

            public IFingerprintUnit With(IFingerprintingConfiguration configuration)
            {
                return new EmptyFingerprintUnit(fingerprint, minHashService);
            }

            public IFingerprintUnit With<T>() where T : IFingerprintingConfiguration, new()
            {
                return new EmptyFingerprintUnit(fingerprint, minHashService);
            }

            public IFingerprintUnit WithCustomConfiguration(Action<CustomFingerprintingConfiguration> functor)
            {
                return new EmptyFingerprintUnit(fingerprint, minHashService);
            }

            public IFingerprintUnit WithDefaultConfiguration()
            {
                return new EmptyFingerprintUnit(fingerprint, minHashService);
            }

            private class EmptyFingerprintUnit : IFingerprintUnit
            {
                private readonly bool[] fingerprint;

                private readonly IMinHashService minHashService;

                public EmptyFingerprintUnit(bool[] fingerprint, IMinHashService minHashService)
                {
                    this.fingerprint = fingerprint;
                    this.minHashService = minHashService;
                }

                public IFingerprintingConfiguration Configuration { get; set; }

                public Task<List<bool[]>> RunAlgorithm()
                {
                    TaskCompletionSource<List<bool[]>> tcs = new TaskCompletionSource<List<bool[]>>();
                    tcs.SetResult(new List<bool[]> { fingerprint });
                    return tcs.Task;
                }

                public Task<List<bool[]>> RunAlgorithm(CancellationToken token)
                {
                    return RunAlgorithm();
                }

                public Task<List<byte[]>> RunAlgorithmWithHashing()
                {
                    TaskCompletionSource<List<byte[]>> tcs = new TaskCompletionSource<List<byte[]>>();
                    tcs.SetResult(new List<byte[]> { minHashService.Hash(fingerprint) });
                    return tcs.Task;
                }

                public Task<List<byte[]>> RunAlgorithmWithHashing(CancellationToken token)
                {
                    return RunAlgorithmWithHashing();
                }
            }
        }
    }
}