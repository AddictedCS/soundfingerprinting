namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.LSH;

    public class FingerprintCommandBuilder : IFingerprintCommandBuilder
    {
        private readonly IFingerprintService fingerprintService;
        private readonly IAudioService audioService;
        private readonly ILocalitySensitiveHashingAlgorithm lshAlgorithm;

        public FingerprintCommandBuilder()
            : this(
                DependencyResolver.Current.Get<IFingerprintService>(),
                DependencyResolver.Current.Get<IAudioService>(),
                DependencyResolver.Current.Get<ILocalitySensitiveHashingAlgorithm>())
        {
        }

        public FingerprintCommandBuilder(IFingerprintService fingerprintService, IAudioService audioService, ILocalitySensitiveHashingAlgorithm lshAlgorithm)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
            this.lshAlgorithm = lshAlgorithm;
        }

        public ISourceFrom BuildFingerprintCommand()
        {
            return new FingerprintCommand(fingerprintService, audioService, lshAlgorithm);
        }
    }
}