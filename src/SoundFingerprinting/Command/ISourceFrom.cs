namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;

    using SoundFingerprinting.Audio;

    public interface ISourceFrom
    {
        IWithFingerprintConfiguration From(string pathToAudioFile);

        IWithFingerprintConfiguration From(AudioSamples audioSamples);

        IWithFingerprintConfiguration From(IEnumerable<bool[]> fingerprints);

        IWithFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond);
    }
}