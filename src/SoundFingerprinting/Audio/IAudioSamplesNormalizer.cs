namespace SoundFingerprinting.Audio
{
    internal interface IAudioSamplesNormalizer
    {
        void NormalizeInPlace(float[] samples);

        void NormalizeInPlace(float[] samples, int sampleRate, int windowInSeconds);
    }
}