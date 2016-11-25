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

    public sealed class FingerprintCommand : ISourceFrom, IWithFingerprintConfiguration, IFingerprintCommand
    {
        private readonly ILocalitySensitiveHashingAlgorithm lshAlgorithm;
        private readonly IFingerprintService fingerprintService;

        private Func<List<Fingerprint>> createFingerprintsMethod;

        private IAudioService audioService;

        internal FingerprintCommand(IFingerprintService fingerprintService, ILocalitySensitiveHashingAlgorithm lshAlgorithm)
        {
            this.fingerprintService = fingerprintService;
            this.lshAlgorithm = lshAlgorithm;
            FingerprintConfiguration = new DefaultFingerprintConfiguration();
        }

        public FingerprintConfiguration FingerprintConfiguration { get; private set; }

                public Task<List<HashedFingerprint>> Hash()
        {
            return Task.Factory
                .StartNew(createFingerprintsMethod)
                .ContinueWith(fingerprintsResult => HashFingerprints(fingerprintsResult.Result), TaskContinuationOptions.ExecuteSynchronously);
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile)
        {
            createFingerprintsMethod = () =>
                {
                    AudioSamples audioSamples = audioService.ReadMonoSamplesFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate);
                    return fingerprintService.CreateFingerprints(audioSamples, FingerprintConfiguration);
                };

            return this;
        }

        public IWithFingerprintConfiguration From(AudioSamples audioSamples)
        {
            createFingerprintsMethod = () => fingerprintService.CreateFingerprints(audioSamples, FingerprintConfiguration);
            return this;
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile, double secondsToProcess, double startAtSecond)
        {
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

        public IUsingFingerprintServices WithFingerprintConfig(Action<FingerprintConfiguration> amendFunctor)
        {
            amendFunctor(FingerprintConfiguration);
            return this;
        }

        public IFingerprintCommand UsingServices(IAudioService audioServiceToUse)
        {
            this.audioService = audioServiceToUse;
            return this;
        }

        internal Task<List<Fingerprint>> Fingerprint()
        {
            return Task.Factory.StartNew(createFingerprintsMethod);
        }

        private List<HashedFingerprint> HashFingerprints(IEnumerable<Fingerprint> fingerprints)
        {
            var hashedFingerprints = new ConcurrentBag<HashedFingerprint>();
            Parallel.ForEach(
                fingerprints,
                (fingerprint, state, index) =>
                    {
                        var hashedFingerprint = lshAlgorithm.Hash(
                            fingerprint,
                            FingerprintConfiguration.HashingConfig.NumberOfLSHTables,
                            FingerprintConfiguration.HashingConfig.NumberOfMinHashesPerTable);
                        hashedFingerprint.AssignedClusters = FingerprintConfiguration.AssignedClusters;
                        hashedFingerprints.Add(hashedFingerprint);
                    });

            return hashedFingerprints.ToList();
        }
    }
}