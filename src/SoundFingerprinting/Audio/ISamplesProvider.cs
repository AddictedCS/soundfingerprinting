namespace SoundFingerprinting.Audio
{
    public interface ISamplesProvider
    {
        int GetNextSamples(float[] buffer);
    }
}
