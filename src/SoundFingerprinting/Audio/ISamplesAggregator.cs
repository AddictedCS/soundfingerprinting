namespace SoundFingerprinting.Audio
{
    public interface ISamplesAggregator
    {
        float[] ReadSamplesFromSource(ISamplesProvider samplesProvider, double secondsToRead, int sampleRate);
    }
}