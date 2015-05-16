namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Data;

    /// <summary>
    /// Source from object which allows you to select the source to build the fingerprints from
    /// </summary>
    public interface ISourceFrom
    {
        IWithFingerprintConfiguration From(string pathToAudioFile);

        IWithFingerprintConfiguration From(AudioSamples audioSamples);

        IWithFingerprintConfiguration From(IEnumerable<Fingerprint> fingerprints);

        IWithFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond);
    }
}