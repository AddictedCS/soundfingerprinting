namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public interface IFingerprintService
    {
        Hashes CreateFingerprintsFromAudioSamples(AudioSamples samples, FingerprintConfiguration configuration);

        Hashes CreateFingerprintsFromImageFrames(IEnumerable<Frame> imageFrames, FingerprintConfiguration configuration);
    }
}