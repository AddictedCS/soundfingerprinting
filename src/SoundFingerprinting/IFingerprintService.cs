namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public interface IFingerprintService
    {
        List<Fingerprint> CreateFingerprints(AudioSamples samples, FingerprintConfiguration configuration);
    }
}