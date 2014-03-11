namespace SoundFingerprinting.Audio.Bass
{
    using System;

    internal interface IBassResampler
    {
        float[] Resample(int sourceStream, int sampleRate, int seconds, int startAt, Func<int, ISamplesProvider> getSamplesProvider);
    }
}