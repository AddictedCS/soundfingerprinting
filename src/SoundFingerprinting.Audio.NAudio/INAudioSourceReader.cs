namespace SoundFingerprinting.Audio.NAudio
{
    internal interface INAudioSourceReader
    {
        float[] ReadMonoFromSource(string source, int sampleRate, int secondsToRead, int startAtSecond);
    }
}