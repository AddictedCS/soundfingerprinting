namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;

    public interface IAudioService
    {
        IReadOnlyCollection<string> SupportedFormats { get; }

        float[] ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, int seconds, int startAt);

        float[] ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate);
    }
}