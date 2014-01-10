namespace SoundFingerprinting.Command
{
    public interface ISourceFrom
    {
        IWithFingerprintConfiguration From(string pathToAudioFile);

        IWithFingerprintConfiguration From(float[] audioSamples);

        IWithFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond);
    }
}