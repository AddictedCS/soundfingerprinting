namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Hashing;
    using SoundFingerprinting.Infrastructure;

    public class FingerprintCommandBuilder : IFingerprintCommandBuilder
    {
        private readonly IFingerprintService fingerprintService;
        private readonly IAudioService audioService;
        private readonly ILocalitySensitiveHashingAlgorithm localitySensitiveHashingAlgorithm;

        public FingerprintCommandBuilder()
            : this(
                DependencyResolver.Current.Get<IFingerprintService>(),
                DependencyResolver.Current.Get<IAudioService>(),
                DependencyResolver.Current.Get<ILocalitySensitiveHashingAlgorithm>())
        {
        }

        public FingerprintCommandBuilder(IFingerprintService fingerprintService, IAudioService audioService, ILocalitySensitiveHashingAlgorithm localitySensitiveHashingAlgorithm)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
            this.localitySensitiveHashingAlgorithm = localitySensitiveHashingAlgorithm;
        }

        public ISourceFrom BuildFingerprintCommand()
        {
            return new FingerprintCommand(fingerprintService, audioService, localitySensitiveHashingAlgorithm);
        }
    }
}