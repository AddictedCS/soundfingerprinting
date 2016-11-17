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

    internal sealed class FingerprintCommand : ISourceFrom, IWithFingerprintConfiguration, IFingerprintCommand
    {
        private readonly ILocalitySensitiveHashingAlgorithm lshAlgorithm;
        private readonly IFingerprintService fingerprintService;

        private Func<Tuple<List<Fingerprint>, double>> createFingerprintsMethod;

        private IAudioService audioService;

        public FingerprintCommand(IFingerprintService fingerprintService, ILocalitySensitiveHashingAlgorithm lshAlgorithm)
        {
            this.fingerprintService = fingerprintService;
            this.lshAlgorithm = lshAlgorithm;
            FingerprintConfiguration = new DefaultFingerprintConfiguration();
        }

        public FingerprintConfiguration FingerprintConfiguration { get; private set; }

        public Task<List<Fingerprint>> Fingerprint()
        {
            return Task.Factory.StartNew(() => this.createFingerprintsMethod().Item1);
        }

        public Task<List<HashedFingerprint>> Hash()
        {
            return Task.Factory
                .StartNew(createFingerprintsMethod)
                .ContinueWith(fingerprintsResult => HashFingerprints(fingerprintsResult.Result.Item1, fingerprintsResult.Result.Item2), TaskContinuationOptions.ExecuteSynchronously);
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile)
        {
            createFingerprintsMethod = () =>
                {
                    AudioSamples audioSamples = audioService.ReadMonoSamplesFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate);
                    return new Tuple<List<Fingerprint>, double>(fingerprintService.CreateFingerprints(audioSamples, FingerprintConfiguration), audioSamples.Duration);
                };

            return this;
        }

        public IWithFingerprintConfiguration From(AudioSamples audioSamples)
        {
            createFingerprintsMethod = () => new Tuple<List<Fingerprint>, double>(fingerprintService.CreateFingerprints(audioSamples, FingerprintConfiguration), audioSamples.Duration);
            return this;
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile, double secondsToProcess, double startAtSecond)
        {
            createFingerprintsMethod = () =>
                {
                    AudioSamples audioSamples = audioService.ReadMonoSamplesFromFile(pathToAudioFile, FingerprintConfiguration.SampleRate, secondsToProcess, startAtSecond);
                    return new Tuple<List<Fingerprint>, double>(fingerprintService.CreateFingerprints(audioSamples, FingerprintConfiguration), audioSamples.Duration);
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

        private List<HashedFingerprint> HashFingerprints(IEnumerable<Fingerprint> fingerprints, double sourceDuration)
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
                        hashedFingerprint.SourceDuration = sourceDuration;
                        hashedFingerprints.Add(hashedFingerprint);
                    });

            return hashedFingerprints.ToList();
        }
    }
}