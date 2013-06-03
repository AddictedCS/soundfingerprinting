namespace Soundfingerprinting.Fingerprinting.FingerprintUnitBuilder
{
    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Fingerprinting.FingerprintUnitBuilder.Internal;
    using Soundfingerprinting.Hashing.MinHash;
    using Soundfingerprinting.Infrastructure;

    public class FingerprintingUnitsBuilder : IFingerprintingUnitsBuilder
    {
        private readonly IFingerprintService fingerprintService;
        private readonly IAudioService audioService;
        private readonly IMinHashService minHashService;

        public FingerprintingUnitsBuilder()
            : this(DependencyResolver.Current.Get<IFingerprintService>(), DependencyResolver.Current.Get<IAudioService>(), DependencyResolver.Current.Get<IMinHashService>())
        {
        }

        public FingerprintingUnitsBuilder(IFingerprintService fingerprintService, IAudioService audioService, IMinHashService minHashService)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
            this.minHashService = minHashService;
        }

        public ITargetOn BuildFingerprints()
        {
            return new FingerprintingUnit(fingerprintService, audioService, minHashService);
        }
    }
}