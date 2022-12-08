namespace SoundFingerprinting.Command
{
    using System;
    using System.Threading.Tasks;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Media;
    using SoundFingerprinting.Video;

    /// <summary>
    ///  Class handling executed fingerprinting command.
    /// </summary>
    public class ExecutedFingerprintCommand : IWithFingerprintConfiguration
    {
        private readonly AVHashes result;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutedFingerprintCommand"/> class.
        /// </summary>
        /// <param name="result">Hashed result.</param>
        public ExecutedFingerprintCommand(AVHashes result)
        {
            this.result = result;
        }

        public Task<AVHashes> Hash()
        {
            return Task.FromResult(result);
        }

        public IFingerprintCommand UsingServices(IAudioService audioService)
        {
            return this;
        }

        public IFingerprintCommand UsingServices(IVideoService videoService)
        {
            return this;
        }

        public IFingerprintCommand UsingServices(IMediaService mediaService)
        {
            return this;
        }

        public IUsingFingerprintServices WithFingerprintConfig(AVFingerprintConfiguration configuration)
        {
            return this;
        }

        public IUsingFingerprintServices WithFingerprintConfig(Func<AVFingerprintConfiguration, AVFingerprintConfiguration> amendFunctor)
        {
            return this;
        }
    }
}