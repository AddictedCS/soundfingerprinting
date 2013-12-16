namespace SoundFingerprinting
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Hashing.LSH;
    using SoundFingerprinting.Hashing.MinHash;
    using SoundFingerprinting.Infrastructure;

    public class FingerprintCommandBuilder : IFingerprintCommandBuilder
    {
        private readonly IFingerprintService fingerprintService;
        private readonly IAudioService audioService;
        private readonly IMinHashService minHashService;
        private readonly ILSHService lshService;

        public FingerprintCommandBuilder()
            : this(
                DependencyResolver.Current.Get<IFingerprintService>(),
                DependencyResolver.Current.Get<IAudioService>(),
                DependencyResolver.Current.Get<IMinHashService>(),
                DependencyResolver.Current.Get<ILSHService>())
        {
        }

        public FingerprintCommandBuilder(IFingerprintService fingerprintService, IAudioService audioService, IMinHashService minHashService, ILSHService lshService)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
            this.minHashService = minHashService;
            this.lshService = lshService;
        }

        public ISourceFrom BuildFingerprintCommand()
        {
            return new FingerprintCommand(fingerprintService, audioService, minHashService, lshService);
        }
    }
}