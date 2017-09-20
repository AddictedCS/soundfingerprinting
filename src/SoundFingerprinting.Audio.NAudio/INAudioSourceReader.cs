namespace SoundFingerprinting.Audio.NAudio
{
    internal interface INAudioSourceReader
    {
        float[] ReadMonoFromSource(string source, int sampleRate, double secondsToRead, double startAtSecond, int resamplerQuality);
    }
}