namespace SoundFingerprinting.Audio
{
    public interface ISamplesAggregator
    {
        float[] ReadSamplesFromSource(ISamplesProvider samplesProvider, int secondsToRead, int sampleRate);
    }
}