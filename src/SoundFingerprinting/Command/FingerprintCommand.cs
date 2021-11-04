namespace SoundFingerprinting.Command
{
    using System;
    using System.Threading.Tasks;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    /// <summary>
    ///  Fingerprint command class that configures and builds the fingerprints from a source content file.
    /// </summary>
    /// <remarks>
    ///  Create and configure with <see cref="FingerprintCommandBuilder"/> class.
    /// </remarks>
    public sealed class FingerprintCommand : ISourceFrom, IWithFingerprintConfiguration
    {
        private readonly IFingerprintService fingerprintService;

        private Func<Hashes> createFingerprintsMethod;

        private IAudioService audioService;

        private FingerprintConfiguration fingerprintConfiguration;

        internal FingerprintCommand(IFingerprintService fingerprintService)
        {
            this.fingerprintService = fingerprintService;
            audioService = new SoundFingerprintingAudioService();
            fingerprintConfiguration = new DefaultFingerprintConfiguration();
            createFingerprintsMethod = () => Hashes.GetEmpty(MediaType.Audio);
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

        /// <inheritdoc cref="ISourceFrom.From(Frames)"/>
        public IWithFingerprintConfiguration From(Frames frames)
        {
            createFingerprintsMethod = () => fingerprintService.CreateFingerprintsFromImageFrames(frames, fingerprintConfiguration);
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