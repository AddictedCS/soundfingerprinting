namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.LSH;

    internal sealed class FingerprintCommand : ISourceFrom, IWithFingerprintConfiguration, IUsingFingerprintServices, IFingerprintCommand
    {
        private readonly ILocalitySensitiveHashingAlgorithm lshAlgorithm;
        
        private readonly IFingerprintService fingerprintService;

        private Func<List<SpectralImage>> createSpectralImages; 

        private Func<List<bool[]>> createFingerprintsMethod;

        private IAudioService audioService;

        public FingerprintCommand(IFingerprintService fingerprintService, ILocalitySensitiveHashingAlgorithm lshAlgorithm)
        {
            this.fingerprintService = fingerprintService;
            this.lshAlgorithm = lshAlgorithm;
        }

        public FingerprintConfiguration FingerprintConfiguration { get; private set; }

        public Task<List<SpectralImage>> CreateSpectralImages()
        {
            return Task.Factory.StartNew(createSpectralImages);
        }

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
            createSpectralImages = () =>
                {
                    AudioSamples audioSamples = audioService.ReadMonoSamplesFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate);
                    return fingerprintService.CreateSpectralImages(audioSamples, FingerprintConfiguration);
                };

            createFingerprintsMethod = () =>
                {
                    AudioSamples audioSamples = audioService.ReadMonoSamplesFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate);
                    return fingerprintService.CreateFingerprints(audioSamples, FingerprintConfiguration);
                };

            return this;
        }

        public IWithFingerprintConfiguration From(AudioSamples audioSamples)
        {
            createSpectralImages = () => fingerprintService.CreateSpectralImages(audioSamples, FingerprintConfiguration);
            createFingerprintsMethod = () => fingerprintService.CreateFingerprints(audioSamples, FingerprintConfiguration);
            return this;
        }

        public IWithFingerprintConfiguration From(IEnumerable<bool[]> fingerprints)
        {
            createSpectralImages = () => { throw new Exception("Could not create spectral images from fingerprinted content"); };
            createFingerprintsMethod = fingerprints.ToList;
            return this;
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond)
        {
            createSpectralImages = () =>
                {
                    AudioSamples audioSamples = audioService.ReadMonoSamplesFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate, secondsToProcess, startAtSecond);
                    return fingerprintService.CreateSpectralImages(audioSamples, FingerprintConfiguration);
                };

            createFingerprintsMethod = () =>
                {
                    AudioSamples audioSamples = audioService.ReadMonoSamplesFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate, secondsToProcess, startAtSecond);
                    return fingerprintService.CreateFingerprints(audioSamples, FingerprintConfiguration);
                };

            return this;
        }

        public IUsingFingerprintServices WithFingerprintConfig(FingerprintConfiguration configuration)
        {
            FingerprintConfiguration = configuration;
            return this;
        }

        public IUsingFingerprintServices WithFingerprintConfig<T>() where T : FingerprintConfiguration, new()
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
                (fingerprint, state, index) =>
                    {
                        var hashData = lshAlgorithm.Hash(
                            fingerprint,
                            FingerprintConfiguration.HashingConfig.NumberOfLSHTables,
                            FingerprintConfiguration.HashingConfig.NumberOfMinHashesPerTable);
                        hashData.SequenceNumber = (int)index + 1;
                        hashDatas.Add(hashData);
                    });

            return hashDatas.ToList();
        }
    }
}