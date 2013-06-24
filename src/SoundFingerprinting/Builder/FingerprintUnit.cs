namespace SoundFingerprinting.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Hashing.MinHash;

    internal sealed class FingerprintUnit : ITargetOn, IWithFingerprintConfiguration, IFingerprintUnit
    {
        private readonly IAudioService audioService;
        private readonly IMinHashService minHashService;
        private readonly IFingerprintService fingerprintService;
        private CancellationToken cancellationToken;

        private Func<List<bool[]>> createFingerprintsMethod;

        public FingerprintUnit(IFingerprintService fingerprintService, IAudioService audioService, IMinHashService minHashService)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
            this.minHashService = minHashService;
        }

        public IFingerprintingConfiguration Configuration { get; private set; }

        public Task<List<bool[]>> RunAlgorithm()
        {
            return Task.Factory.StartNew(createFingerprintsMethod);
        }

        public Task<List<bool[]>> RunAlgorithm(CancellationToken token)
        {
            return Task.Factory.StartNew(createFingerprintsMethod, token);
        }

        public Task<List<byte[]>> RunAlgorithmWithHashing()
        {
            return Task.Factory.StartNew(createFingerprintsMethod)
                               .ContinueWith(task => HashFingerprints(task.Result));
        }

        public Task<List<byte[]>> RunAlgorithmWithHashing(CancellationToken token)
        {
            cancellationToken = token;
            return Task.Factory.StartNew(createFingerprintsMethod, token)
                              .ContinueWith(task => HashFingerprints(task.Result));
        }

        public IWithFingerprintConfiguration On(string pathToAudioFile)
        {
            return On(pathToAudioFile, 0, 0);
        }

        public IWithFingerprintConfiguration On(float[] audioSamples)
        {
            createFingerprintsMethod = () => fingerprintService.CreateFingerprints(audioSamples, Configuration);
            return this;
        }

        public IWithFingerprintConfiguration On(string pathToAudioFile, int secondsToProcess, int startAtSecond)
        {
            createFingerprintsMethod = () =>
                {
                    float[] samples = audioService.ReadMonoFromFile(pathToAudioFile, Configuration.SampleRate, secondsToProcess, startAtSecond);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new List<bool[]>(Enumerable.Empty<bool[]>());
                    }

                    return fingerprintService.CreateFingerprints(samples, Configuration);
                };
            return this;
        }

        public IFingerprintUnit With(IFingerprintingConfiguration configuration)
        {
            Configuration = configuration;
            return this;
        }

        public IFingerprintUnit With<T>() where T : IFingerprintingConfiguration, new()
        {
            Configuration = new T();
            return this;
        }

        public IFingerprintUnit WithCustomConfiguration(Action<CustomFingerprintingConfiguration> functor)
        {
            CustomFingerprintingConfiguration customFingerprintingConfiguration = new CustomFingerprintingConfiguration();
            Configuration = customFingerprintingConfiguration;
            functor(customFingerprintingConfiguration);
            return this;
        }

        public IFingerprintUnit WithDefaultConfiguration()
        {
            Configuration = new DefaultFingerprintingConfiguration();
            return this;
        }

        private List<byte[]> HashFingerprints(IEnumerable<bool[]> fingerprints)
        {
            List<byte[]> subFingerprints = new List<byte[]>();
            foreach (var fingerprint in fingerprints)
            {
                subFingerprints.Add(minHashService.Hash(fingerprint));
            }

            return subFingerprints;
        }
    }
}