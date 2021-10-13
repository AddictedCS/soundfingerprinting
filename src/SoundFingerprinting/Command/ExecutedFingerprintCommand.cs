namespace SoundFingerprinting.Command
{
    using System;
    using System.Threading.Tasks;
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public class ExecutedFingerprintCommand : IWithFingerprintConfiguration
    {
        private readonly Hashes result;
        
        public ExecutedFingerprintCommand(Hashes result)
        {
            this.result = result;
        }

        public Task<Hashes> Hash()
        {
            return Task.FromResult(result);
        }

        public IFingerprintCommand UsingServices(IAudioService audioService)
        {
            return this;
        }

        public IUsingFingerprintServices WithFingerprintConfig(FingerprintConfiguration configuration)
        {
            return this;
        }

        public IUsingFingerprintServices WithFingerprintConfig(Func<FingerprintConfiguration, FingerprintConfiguration> amendFunctor)
        {
            return this;
        }
    }
}