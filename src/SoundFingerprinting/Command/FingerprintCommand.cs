namespace SoundFingerprinting.Command
{
    using System;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public sealed class FingerprintCommand : ISourceFrom, IWithFingerprintConfiguration, IFingerprintCommand
    {
        private readonly IFingerprintService fingerprintService;

        private Func<Hashes> createFingerprintsMethod;

        private IAudioService audioService;

        private FingerprintConfiguration fingerprintConfiguration;

        public FingerprintCommand(IFingerprintService fingerprintService)
        {
            this.fingerprintService = fingerprintService;
            fingerprintConfiguration = new DefaultFingerprintConfiguration();
        }

        public Task<Hashes> Hash()
        {
            return Task.Factory.StartNew(createFingerprintsMethod);
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile)
        {
            createFingerprintsMethod = () =>
                {
                    AudioSamples audioSamples = audioService.ReadMonoSamplesFromFile(pathToAudioFile, fingerprintConfiguration.SampleRate);
                    return fingerprintService.CreateFingerprintsFromAudioSamples(audioSamples, fingerprintConfiguration);
                };

            return this;
        }

        public IWithFingerprintConfiguration From(AudioSamples audioSamples)
        {
            createFingerprintsMethod = () => fingerprintService.CreateFingerprintsFromAudioSamples(audioSamples, fingerprintConfiguration);
            return this;
        }

        public IWithFingerprintConfiguration From(string pathToAudioFile, double secondsToProcess, double startAtSecond)
        {
            createFingerprintsMethod = () =>
                {
                    AudioSamples audioSamples = audioService.ReadMonoSamplesFromFile(pathToAudioFile, fingerprintConfiguration.SampleRate, secondsToProcess, startAtSecond);
                    return fingerprintService.CreateFingerprintsFromAudioSamples(audioSamples, fingerprintConfiguration);
                };

            return this;
        }

        public IUsingFingerprintServices WithFingerprintConfig(FingerprintConfiguration configuration)
        {
            fingerprintConfiguration = configuration;
            return this;
        }

        public IUsingFingerprintServices WithFingerprintConfig(Func<FingerprintConfiguration, FingerprintConfiguration> amendFunctor)
        {
            fingerprintConfiguration = amendFunctor(fingerprintConfiguration);
            return this;
        }

        public IFingerprintCommand UsingServices(IAudioService audioServiceToUse)
        {
            audioService = audioServiceToUse;
            return this;
        }
    }
}