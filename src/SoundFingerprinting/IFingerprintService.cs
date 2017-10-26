namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    internal interface IFingerprintService
    {
        List<HashedFingerprint> CreateFingerprints(AudioSamples samples, FingerprintConfiguration configuration);
    }
}