namespace Soundfingerprinting.Fingerprinting.FingerprintUnitBuilder
{
    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Fingerprinting.FingerprintUnitBuilder.Internal;
    using Soundfingerprinting.Hashing.MinHash;

    public class FingerprintingUnitsBuilder : IFingerprintingUnitsBuilder
    {
        private readonly IFingerprintService fingerprintService;
        private readonly IAudioService audioService;
        private readonly IMinHashService minHashService;

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