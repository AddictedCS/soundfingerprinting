namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Hashing.MinHash;

    internal sealed class FingerprintingUnit : ITargetOn, IWithConfiguration, IFingerprintingUnit
    {
        private readonly IAudioService audioService;
        private readonly IMinHashService minHashService;
        private readonly IFingerprintService fingerprintService;

        private Func<Task<List<bool[]>>> createFingerprintsMethod;

        public FingerprintingUnit(IFingerprintService fingerprintService, IAudioService audioService, IMinHashService minHashService)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
            this.minHashService = minHashService;
        }

        public IFingerprintingConfiguration Configuration { get; private set; }

        public Task<List<bool[]>> RunAlgorithm()
        {
            return createFingerprintsMethod();
        }

        public Task<List<byte[]>> RunAlgorithmWithHashing()
        {
            return createFingerprintsMethod().ContinueWith(
                task =>
                    {
                        List<bool[]> fingerprints = task.Result;
                        List<byte[]> subFingerprints = new List<byte[]>();
                        foreach (var fingerprint in fingerprints)
                        {
                            subFingerprints.Add(minHashService.Hash(fingerprint));
                        }

                        return subFingerprints;
                    });
        }

        public IWithConfiguration On(string pathToAudioFile)
        {
            createFingerprintsMethod = () =>
                {
                    float[] samples = audioService.ReadMonoFromFile(pathToAudioFile, Configuration.SampleRate, 0, 0);
                    return fingerprintService.CreateFingerprints(samples, Configuration);
                };

            return this;
        }

        public IWithConfiguration On(float[] audioSamples)
        {
            createFingerprintsMethod = () => fingerprintService.CreateFingerprints(audioSamples, Configuration);
            return this;
        }

        public IWithConfiguration On(string pathToAudioFile, int millisecondsToProcess, int startAtMillisecond)
        {
            createFingerprintsMethod = () =>
                {
                    float[] samples = audioService.ReadMonoFromFile(pathToAudioFile, Configuration.SampleRate, millisecondsToProcess, startAtMillisecond);
                    return fingerprintService.CreateFingerprints(samples, Configuration);
                };
            return this;
        }

        public IFingerprintingUnit With(IFingerprintingConfiguration configuration)
        {
            Configuration = configuration;
            return this;
        }

        public IFingerprintingUnit With<T>() where T : IFingerprintingConfiguration, new()
        {
            Configuration = new T();
            return this;
        }

        public IFingerprintingUnit WithCustomConfiguration(Action<CustomFingerprintingConfiguration> transformation)
        {
            CustomFingerprintingConfiguration customFingerprintingConfiguration = new CustomFingerprintingConfiguration();
            Configuration = customFingerprintingConfiguration;
            transformation(customFingerprintingConfiguration);
            return this;
        }
    }
}