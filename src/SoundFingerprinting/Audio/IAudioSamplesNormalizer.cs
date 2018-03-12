namespace SoundFingerprinting.Audio
{
    internal interface IAudioSamplesNormalizer
    {
        void NormalizeInPlace(float[] samples);
    }
}