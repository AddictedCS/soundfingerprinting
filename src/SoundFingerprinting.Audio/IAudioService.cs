namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;

    public interface IAudioService
    {
        IReadOnlyCollection<string> SupportedFormats { get; }

        AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, double seconds, double startAt);

        AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate);
    }
}