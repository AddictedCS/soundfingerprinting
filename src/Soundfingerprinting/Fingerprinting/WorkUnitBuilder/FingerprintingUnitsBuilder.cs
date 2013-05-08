namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder
{
    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder.Internal;

    public class FingerprintingUnitsBuilder : IFingerprintingUnitsBuilder
    {
        private readonly IFingerprintService fingerprintService;
        private readonly IAudioService audioService;

        public FingerprintingUnitsBuilder(IFingerprintService fingerprintService, IAudioService audioService)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
        }

        public ITargetOn BuildFingerprints()
        {
            return new FingerprintingUnit(fingerprintService, audioService);
        }
    }
}