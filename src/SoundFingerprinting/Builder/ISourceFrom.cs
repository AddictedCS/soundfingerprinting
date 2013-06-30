namespace SoundFingerprinting.Builder
{
    public interface ISourceFrom
    {
        IWithAlgorithmConfiguration From(string pathToAudioFile);

        IWithAlgorithmConfiguration From(float[] audioSamples);

        IWithAlgorithmConfiguration From(string pathToAudioFile, int secondsToProcess, int startAtSecond);
    }
}