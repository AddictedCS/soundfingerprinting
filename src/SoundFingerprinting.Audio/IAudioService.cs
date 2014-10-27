namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;

    public interface IAudioService
    {
        IReadOnlyCollection<string> SupportedFormats { get; }

        float[] ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, int seconds, int startAt);

        float[] ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate);
    }

    public abstract class AudioService : IAudioService
    {
        public abstract IReadOnlyCollection<string> SupportedFormats { get; }

        public abstract float[] ReadMonoSamplesFromFile(
            string pathToSourceFile, int sampleRate, int seconds, int startAt);

        public float[] ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate)
        {
            return ReadMonoSamplesFromFile(pathToSourceFile, sampleRate, 0, 0);
        }
    }
}