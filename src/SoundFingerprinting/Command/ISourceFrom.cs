namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Data;

    public interface ISourceFrom
    {
        IWithFingerprintConfiguration From(string pathToAudioFile);

        IWithFingerprintConfiguration From(AudioSamples audioSamples);

        IWithFingerprintConfiguration From(IEnumerable<Fingerprint> fingerprints);

        IWithFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond);
    }
}