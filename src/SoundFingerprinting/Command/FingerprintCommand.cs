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
    using SoundFingerprinting.LSH;

    internal sealed class FingerprintCommand : ISourceFrom, IWithFingerprintConfiguration, IUsingFingerprintServices, IFingerprintCommand
    {
        private readonly ILocalitySensitiveHashingAlgorithm lshAlgorithm;
        
        private readonly IFingerprintService fingerprintService;

        private Func<List<bool[]>> createFingerprintsMethod;

        private IAudioService audioService;

        public FingerprintCommand(IFingerprintService fingerprintService, ILocalitySensitiveHashingAlgorithm lshAlgorithm)
        {
            this.fingerprintService = fingerprintService;
            this.lshAlgorithm = lshAlgorithm;
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
                    float[] samples = audioService.ReadMonoSamplesFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate, 0, 0);
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
                    float[] samples = audioService.ReadMonoSamplesFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate, secondsToProcess, startAtSecond);
                    return fingerprintService.CreateFingerprints(samples, FingerprintConfiguration);
                };

            return this;
        }

        public IUsingFingerprintServices WithFingerprintConfig(IFingerprintConfiguration configuration)
        {
            FingerprintConfiguration = configuration;
            return this;
        }

        public IUsingFingerprintServices WithFingerprintConfig<T>() where T : IFingerprintConfiguration, new()
        {
            FingerprintConfiguration = new T();
            return this;
        }

        public IUsingFingerprintServices WithFingerprintConfig(Action<CustomFingerprintConfiguration> functor)
        {
            CustomFingerprintConfiguration customFingerprintConfiguration = new CustomFingerprintConfiguration();
            FingerprintConfiguration = customFingerprintConfiguration;
            functor(customFingerprintConfiguration);
            return this;
        }

        public IUsingFingerprintServices WithDefaultFingerprintConfig()
        {
            FingerprintConfiguration = new DefaultFingerprintConfiguration();
            return this;
        }

        public IFingerprintCommand UsingServices(IAudioService audioService)
        {
            this.audioService = audioService;
            return this;
        }

        private List<HashData> HashFingerprints(IEnumerable<bool[]> fingerprints)
        {
            var hashDatas = new ConcurrentBag<HashData>();
            Parallel.ForEach(
                fingerprints,
                fingerprint =>
                    {
                        var hashData = lshAlgorithm.Hash(
                            fingerprint,
                            FingerprintConfiguration.NumberOfLSHTables,
                            FingerprintConfiguration.NumberOfMinHashesPerTable);
                        hashDatas.Add(hashData);
                    });

            return hashDatas.ToList();
        }
    }
}