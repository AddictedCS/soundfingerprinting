namespace Soundfingerprinting.Query.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.FingerprintUnitBuilder;
    using Soundfingerprinting.Hashing.MinHash;
    using Soundfingerprinting.Query.Configuration;

    internal sealed class FingerprintingQueryUnit : IOngoingQuery, IOngoingQueryConfiguration, IOngoingQueryConfigurationWithFingerprinting, IFingerprintingQueryUnit
    {
        private readonly IFingerprintingUnitsBuilder fingerprintingUnitsBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;
        private readonly IMinHashService minHashService;

        private Func<IWithConfiguration> fingerprintingMethodFromSelector;
        private Func<IFingerprintingUnit> createFingerprintMethod;
        private IQueryConfiguration queryConfiguration;

        public FingerprintingQueryUnit(IFingerprintingUnitsBuilder fingerprintingUnitsBuilder, IQueryFingerprintService queryFingerprintService, IMinHashService minHashService)
        {
            this.fingerprintingUnitsBuilder = fingerprintingUnitsBuilder;
            this.queryFingerprintService = queryFingerprintService;
            this.minHashService = minHashService;
        }

        public IOngoingQueryConfigurationWithFingerprinting From(string pathToAudioFile)
        {
            fingerprintingMethodFromSelector = () => fingerprintingUnitsBuilder.BuildFingerprints().On(pathToAudioFile);
            return this;
        }

        public IOngoingQueryConfigurationWithFingerprinting From(string pathToAudioFile, int millisecondsToProcess, int startAtMillisecond)
        {
            fingerprintingMethodFromSelector = () => fingerprintingUnitsBuilder.BuildFingerprints().On(pathToAudioFile, millisecondsToProcess, startAtMillisecond);
            return this;
        }

        public IOngoingQueryConfigurationWithFingerprinting From(float[] audioSamples)
        {
            fingerprintingMethodFromSelector = () => fingerprintingUnitsBuilder.BuildFingerprints().On(audioSamples);
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

            public IFingerprintingUnit With(IFingerprintingConfiguration configuration)
            {
                return new EmptyFingerprintingUnit(fingerprint, minHashService);
            }

            public IFingerprintingUnit With<T>() where T : IFingerprintingConfiguration, new()
            {
                return new EmptyFingerprintingUnit(fingerprint, minHashService);
            }

            public IFingerprintingUnit WithCustomConfiguration(Action<CustomFingerprintingConfiguration> transformation)
            {
                return new EmptyFingerprintingUnit(fingerprint, minHashService);
            }

            private class EmptyFingerprintingUnit : IFingerprintingUnit
            {
                private readonly bool[] fingerprint;

                private readonly IMinHashService minHashService;

                public EmptyFingerprintingUnit(bool[] fingerprint, IMinHashService minHashService)
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