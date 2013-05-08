namespace Soundfingerprinting.Query.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;

    internal sealed class FingerprintingQueryUnit : IOngoingQuery, IOngoingQueryConfiguration, IOngoingQueryConfigurationWithFingerprinting, IFingerprintingQueryUnit
    {
        private readonly IFingerprintingUnitsBuilder fingerprintingUnitsBuilder;
        private readonly IQueryFingerprintService queryFingerprintService;

        private Func<IWithConfiguration> fingerprintingMethodFromSelector;
        private Func<IFingerprintingUnit> createFingerprintMethod;
        private IQueryConfiguration queryConfiguration;

        public FingerprintingQueryUnit(IFingerprintingUnitsBuilder fingerprintingUnitsBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.fingerprintingUnitsBuilder = fingerprintingUnitsBuilder;
            this.queryFingerprintService = queryFingerprintService;
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
            fingerprintingMethodFromSelector = () => new EmptyWithConfiguration(fingerprint);
            return this;
        }

        public IFingerprintingQueryUnit With(IFingerprintingConfiguration fingerprintingConfiguration, IQueryConfiguration configuration)
        {
            queryConfiguration = configuration;
            createFingerprintMethod = () => fingerprintingMethodFromSelector().With(fingerprintingConfiguration);
            return this;
        }

        public Task<QueryResult> Query()
        {
            return createFingerprintMethod().RunAlgorithm().ContinueWith(task =>
                    {
                        List<bool[]> fingerprints = task.Result;
                        return queryFingerprintService.Query(fingerprints, queryConfiguration).Result;
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

            public EmptyWithConfiguration(bool[] fingerprint)
            {
                this.fingerprint = fingerprint;
            }

            public IFingerprintingUnit With(IFingerprintingConfiguration configuration)
            {
                return new EmptyFingerprintingUnit(fingerprint);
            }

            public IFingerprintingUnit With<T>() where T : IFingerprintingConfiguration, new()
            {
                return new EmptyFingerprintingUnit(fingerprint);
            }

            public IFingerprintingUnit WithCustomConfiguration(Action<CustomFingerprintingConfiguration> transformation)
            {
                return new EmptyFingerprintingUnit(fingerprint);
            }

            private class EmptyFingerprintingUnit : IFingerprintingUnit
            {
                private readonly bool[] fingerprint;

                public EmptyFingerprintingUnit(bool[] fingerprint)
                {
                    this.fingerprint = fingerprint;
                }

                public IFingerprintingConfiguration Configuration { get; set; }

                public Task<List<bool[]>> RunAlgorithm()
                {
                    TaskCompletionSource<List<bool[]>> tcs = new TaskCompletionSource<List<bool[]>>();
                    tcs.SetResult(new List<bool[]> { fingerprint });
                    return tcs.Task;
                }
            }
        }
    }
}