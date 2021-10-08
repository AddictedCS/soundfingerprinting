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

        /// <inheritdoc cref="IFingerprintCommand.Hash"/>
        public Task<Hashes> Hash()
        {
            return Task.Factory.StartNew(createFingerprintsMethod);
        }

        /// <inheritdoc cref="ISourceFrom.From(string)"/>
        public IWithFingerprintConfiguration From(string file)
        {
            createFingerprintsMethod = () =>
                {
                    AudioSamples audioSamples = audioService.ReadMonoSamplesFromFile(file, fingerprintConfiguration.SampleRate);
                    return fingerprintService.CreateFingerprintsFromAudioSamples(audioSamples, fingerprintConfiguration);
                };

            return this;
        }

        /// <inheritdoc cref="ISourceFrom.From(AudioSamples)"/>
        public IWithFingerprintConfiguration From(AudioSamples audioSamples)
        {
            createFingerprintsMethod = () => fingerprintService.CreateFingerprintsFromAudioSamples(audioSamples, fingerprintConfiguration);
            return this;
        }

        /// <inheritdoc cref="ISourceFrom.From(string,double,double)"/>
        public IWithFingerprintConfiguration From(string file, double secondsToProcess, double startAtSecond)
        {
            createFingerprintsMethod = () =>
                {
                    AudioSamples audioSamples = audioService.ReadMonoSamplesFromFile(file, fingerprintConfiguration.SampleRate, secondsToProcess, startAtSecond);
                    return fingerprintService.CreateFingerprintsFromAudioSamples(audioSamples, fingerprintConfiguration);
                };

            return this;
        }

        /// <inheritdoc cref="IWithFingerprintConfiguration.WithFingerprintConfig(FingerprintConfiguration)"/>
        public IUsingFingerprintServices WithFingerprintConfig(FingerprintConfiguration configuration)
        {
            fingerprintConfiguration = configuration;
            return this;
        }

        /// <inheritdoc cref="IWithFingerprintConfiguration.WithFingerprintConfig(Func{FingerprintConfiguration,FingerprintConfiguration})"/>
        public IUsingFingerprintServices WithFingerprintConfig(Func<FingerprintConfiguration, FingerprintConfiguration> amendFunctor)
        {
            fingerprintConfiguration = amendFunctor(fingerprintConfiguration);
            return this;
        }

        /// <inheritdoc cref="IUsingFingerprintServices.UsingServices"/>
        public IFingerprintCommand UsingServices(IAudioService audioServiceToUse)
        {
            audioService = audioServiceToUse;
            return this;
        }
    }
}