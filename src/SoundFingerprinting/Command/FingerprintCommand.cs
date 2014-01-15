namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Hashing;
    using SoundFingerprinting.Hashing.MinHash;

    internal sealed class FingerprintCommand : ISourceFrom, IWithFingerprintConfiguration, IFingerprintCommand
    {
        private readonly IAudioService audioService;

        private readonly ILocalitySensitiveHashingAlgorithm localitySensitiveHashingAlgorithm;
        
        private readonly IFingerprintService fingerprintService;

        private Func<List<bool[]>> createFingerprintsMethod;

        public FingerprintCommand(IFingerprintService fingerprintService, IAudioService audioService, ILocalitySensitiveHashingAlgorithm localitySensitiveHashingAlgorithm)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
            this.localitySensitiveHashingAlgorithm = localitySensitiveHashingAlgorithm;
        }

        public IFingerprintConfiguration FingerprintConfiguration { get; private set; }

        public Task<List<bool[]>> Fingerprint()
        {
            return Task.Factory.StartNew(createFingerprintsMethod);
        }

        public Task<List<HashData>> Hash()
        {
            return Task.Factory
                .StartNew(createFingerprintsMethod)
                .ContinueWith(fingerprintsResult => HashFingerprints(fingerprintsResult.Result), TaskContinuationOptions.ExecuteSynchronously);
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile)
        {
            createFingerprintsMethod = () =>
                {
                    float[] samples = audioService.ReadMonoFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate, 0, 0);
                    return fingerprintService.CreateFingerprints(samples, FingerprintConfiguration);
                };

            return this;
        }

        public IWithFingerprintConfiguration From(float[] audioSamples)
        {
            createFingerprintsMethod = () => fingerprintService.CreateFingerprints(audioSamples, FingerprintConfiguration);
            return this;
        }

        public IWithFingerprintConfiguration From(IEnumerable<bool[]> fingerprints)
        {
            createFingerprintsMethod = fingerprints.ToList;
            return this;
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond)
        {
            createFingerprintsMethod = () =>
                {
                    float[] samples = audioService.ReadMonoFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate, secondsToProcess, startAtSecond);
                    return fingerprintService.CreateFingerprints(samples, FingerprintConfiguration);
                };

            return this;
        }

        public IFingerprintCommand WithFingerprintConfig(IFingerprintConfiguration configuration)
        {
            FingerprintConfiguration = configuration;
            return this;
        }

        public IFingerprintCommand WithFingerprintConfig<T>() where T : IFingerprintConfiguration, new()
        {
            FingerprintConfiguration = new T();
            return this;
        }

        public IFingerprintCommand WithFingerprintConfig(Action<CustomFingerprintConfiguration> functor)
        {
            CustomFingerprintConfiguration customFingerprintConfiguration = new CustomFingerprintConfiguration();
            FingerprintConfiguration = customFingerprintConfiguration;
            functor(customFingerprintConfiguration);
            return this;
        }

        public IFingerprintCommand WithDefaultFingerprintConfig()
        {
            FingerprintConfiguration = new DefaultFingerprintConfiguration();
            return this;
        }

        private List<HashData> HashFingerprints(IEnumerable<bool[]> fingerprints)
        {
            var hashDatas = new ConcurrentBag<HashData>();
            Parallel.ForEach(
                fingerprints,
                fingerprint =>
                    {
                        var hashData = localitySensitiveHashingAlgorithm.Hash(
                            fingerprint,
                            FingerprintConfiguration.NumberOfLSHTables,
                            FingerprintConfiguration.NumberOfMinHashesPerTable);
                        hashDatas.Add(hashData);
                    });

            return hashDatas.ToList();
        }
    }
}