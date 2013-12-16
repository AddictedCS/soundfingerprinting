namespace SoundFingerprinting.Query
{
    public interface IQuerySource
    {
        IWithQueryAndFingerprintConfiguration From(string pathToAudioFile);

        IWithQueryAndFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond);

        IWithQueryAndFingerprintConfiguration From(float[] audioSamples);
    }
}