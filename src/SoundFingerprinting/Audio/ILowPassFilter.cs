namespace SoundFingerprinting.Audio
{
    internal interface ILowPassFilter
    {
        float[] FilterAndDownsample(float[] samples, int sourceSampleRate, int targetSampleRate);
    }
}