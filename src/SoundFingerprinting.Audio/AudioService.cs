namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;

    public abstract class AudioService : IAudioService
    {
        public abstract IReadOnlyCollection<string> SupportedFormats { get; }

        public abstract AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, double seconds, double startAt);

        public AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate)
        {
            return ReadMonoSamplesFromFile(pathToSourceFile, sampleRate, 0, 0);
        }
    }
}