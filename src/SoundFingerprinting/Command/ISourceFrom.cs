namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;

    public interface ISourceFrom
    {
        IWithFingerprintConfiguration From(string pathToAudioFile);

        IWithFingerprintConfiguration From(float[] audioSamples);

        IWithFingerprintConfiguration From(IEnumerable<bool[]> fingerprints);

        IWithFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond);
    }
}