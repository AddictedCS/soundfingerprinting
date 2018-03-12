namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public sealed class FingerprintCommand : ISourceFrom, IWithFingerprintConfiguration, IFingerprintCommand
    {
        private readonly IFingerprintService fingerprintService;

        private Func<List<HashedFingerprint>> createFingerprintsMethod;

        private IAudioService audioService;

        internal FingerprintCommand(IFingerprintService fingerprintService)
        {
            this.fingerprintService = fingerprintService;
            FingerprintConfiguration = new DefaultFingerprintConfiguration();
        }

        public FingerprintConfiguration FingerprintConfiguration { get; private set; }

        public Task<List<HashedFingerprint>> Hash()
        {
            return Task.Factory.StartNew(createFingerprintsMethod);
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

        public IUsingFingerprintServices WithFingerprintConfig(Func<FingerprintConfiguration, FingerprintConfiguration> amendFunctor)
        {
            FingerprintConfiguration = amendFunctor(FingerprintConfiguration);
            return this;
        }

        public IFingerprintCommand UsingServices(IAudioService audioServiceToUse)
        {
            audioService = audioServiceToUse;
            return this;
        }
    }
}