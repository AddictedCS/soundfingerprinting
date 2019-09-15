namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public interface IFingerprintService
    {
        IEnumerable<HashedFingerprint> CreateFingerprintsFromAudioSamples(AudioSamples samples, FingerprintConfiguration configuration);

        IEnumerable<HashedFingerprint> CreateFingerprintsFromImageFrames(IEnumerable<Frame> imageFrames, FingerprintConfiguration configuration);
    }
}