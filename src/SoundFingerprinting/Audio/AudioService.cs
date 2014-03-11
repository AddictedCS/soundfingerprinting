namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;

    public abstract class AudioService : IAudioService
    {
        public const int DefaultSampleRate = 44100;

        public const int DefaultBufferLengthInSeconds = 20;

        public abstract bool IsRecordingSupported { get; }

        public abstract IReadOnlyCollection<string> SupportedFormats { get; }

        public abstract float[] ReadMonoFromFile(string pathToSourceFile, int sampleRate, int seconds, int startAt);

        public float[] ReadMonoFromFile(string pathToSourceFile, int sampleRate)
        {
            return ReadMonoFromFile(pathToSourceFile, sampleRate, 0, 0);
        }

        public abstract void RecodeFileToMonoWave(string pathToFile, string pathToRecodedFile, int sampleRate);

        public abstract float[] ReadMonoSamplesFromStreamingUrl(string streamUrl, int sampleRate, int secondsToDownload);

        public abstract float[] ReadMonoSamplesFromMicrophone(int sampleRate, int secondsToRecord);

        public abstract void WriteSamplesToWaveFile(string pathToFile, float[] samples, int sampleRate);
    }
}