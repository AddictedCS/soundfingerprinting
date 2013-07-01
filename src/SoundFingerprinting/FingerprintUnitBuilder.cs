namespace SoundFingerprinting
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Hashing.MinHash;
    using SoundFingerprinting.Infrastructure;

    public class FingerprintUnitBuilder : IFingerprintUnitBuilder
    {
        private readonly IFingerprintService fingerprintService;
        private readonly IAudioService audioService;
        private readonly IMinHashService minHashService;

        public FingerprintUnitBuilder()
            : this(DependencyResolver.Current.Get<IFingerprintService>(), DependencyResolver.Current.Get<IAudioService>(), DependencyResolver.Current.Get<IMinHashService>())
        {
        }

        public FingerprintUnitBuilder(IFingerprintService fingerprintService, IAudioService audioService, IMinHashService minHashService)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
            this.minHashService = minHashService;
        }

        public ISourceFrom BuildAudioFingerprintingUnit()
        {
            return new AudioFingerprintingUnit(fingerprintService, audioService, minHashService);
        }
    }
}