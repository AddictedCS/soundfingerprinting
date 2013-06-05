namespace Soundfingerprinting.Query.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Soundfingerprinting.Builder;
    using Soundfingerprinting.Configuration;
    using Soundfingerprinting.Hashing.MinHash;
    using Soundfingerprinting.Query.Configuration;

    internal sealed class FingerprintingQueryUnit : IOngoingQuery, IOngoingQueryConfiguration, IOngoingQueryConfigurationWithFingerprinting, IFingerprintingQueryUnit
    {
        private readonly IFingerprintUnitBuilder fingerprintUnitBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        private readonly IMinHashService minHashService;

        private Func<IWithConfiguration> fingerprintingMethodFromSelector;
        private Func<IFingerprintUnit> createFingerprintMethod;
        private IQueryConfiguration queryConfiguration;

        public FingerprintingQueryUnit(IFingerprintUnitBuilder fingerprintUnitBuilder, IQueryFingerprintService queryFingerprintService, IMinHashService minHashService)
        {
            this.fingerprintUnitBuilder = fingerprintUnitBuilder;
            this.queryFingerprintService = queryFingerprintService;
            this.minHashService = minHashService;
        }

        public IOngoingQueryConfigurationWithFingerprinting From(string pathToAudioFile)
        {
            fingerprintingMethodFromSelector = () => fingerprintUnitBuilder.BuildFingerprints().On(pathToAudioFile);
            return this;
        }

        public IOngoingQueryConfigurationWithFingerprinting From(string pathToAudioFile, int millisecondsToProcess, int startAtMillisecond)
        {
            fingerprintingMethodFromSelector = () => fingerprintUnitBuilder.BuildFingerprints().On(pathToAudioFile, millisecondsToProcess, startAtMillisecond);
            return this;
        }

        public IOngoingQueryConfigurationWithFingerprinting From(float[] audioSamples)
        {
            fingerprintingMethodFromSelector = () => fingerprintUnitBuilder.BuildFingerprints().On(audioSamples);
            return this;
        }

        public IOngoingQueryConfiguration From(bool[] fingerprint)
        {
            fingerprintingMethodFromSelector = () => new EmptyWithConfiguration(fingerprint, minHashService);
            return this;
        }

        public IFingerprintingQueryUnit With(IFingerprintingConfiguration fingerprintingConfiguration, IQueryConfiguration configuration)
        {
            queryConfiguration = configuration;
            createFingerprintMethod = () => fingerprintingMethodFromSelector().With(fingerprintingConfiguration);
            return this;
        }

        public IFingerprintingQueryUnit With<T1, T2>() where T1 : IFingerprintingConfiguration, new() where T2 : IQueryConfiguration, new()
        {
            queryConfiguration = new T2();
            createFingerprintMethod = () => fingerprintingMethodFromSelector().With<T1>();
            return this;
        }

        public IFingerprintingQueryUnit WithCustomConfigurations(
            Action<CustomFingerprintingConfiguration> fingerprintingConfigurationTransformation, Action<CustomQueryConfiguration> queryConfigurationTransformation)
        {
            CustomQueryConfiguration customQueryConfiguration = new CustomQueryConfiguration();
            queryConfiguration = customQueryConfiguration;
            queryConfigurationTransformation(customQueryConfiguration);
            createFingerprintMethod = () => fingerprintingMethodFromSelector().WithCustomConfiguration(fingerprintingConfigurationTransformation);
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

        public IFingerprintingQueryUnit With(IQueryConfiguration configuration)
        {
            queryConfiguration = configuration;
            return this;
        }

        private class EmptyWithConfiguration : IWithConfiguration
        {
            private readonly bool[] fingerprint;

            private readonly IMinHashService minHashService;

            public EmptyWithConfiguration(bool[] fingerprint, IMinHashService minHashService)
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

            public IFingerprintUnit WithCustomConfiguration(Action<CustomFingerprintingConfiguration> transformation)
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