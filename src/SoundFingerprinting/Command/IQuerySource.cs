namespace SoundFingerprinting.Command
{
    using SoundFingerprinting.Audio;

    public interface IQuerySource
    {
        IWithQueryAndFingerprintConfiguration From(string pathToAudioFile);

        IWithQueryAndFingerprintConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond);

        IWithQueryAndFingerprintConfiguration From(AudioSamples audioSamples);
    }
}