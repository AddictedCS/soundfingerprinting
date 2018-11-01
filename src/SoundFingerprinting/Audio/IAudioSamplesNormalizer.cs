namespace SoundFingerprinting.Audio
{
    public interface IAudioSamplesNormalizer
    {
        void NormalizeInPlace(float[] samples);

        void NormalizeInPlace(float[] samples, int sampleRate, int windowInSeconds);
    }
}