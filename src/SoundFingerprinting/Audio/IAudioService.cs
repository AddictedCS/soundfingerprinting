namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;

    public interface IAudioService
    {
        bool IsRecordingSupported { get; }

        IReadOnlyCollection<string> SupportedFormats { get; }

        float[] ReadMonoFromFile(string pathToSourceFile, int sampleRate, int seconds, int startAt);

        float[] ReadMonoFromFile(string pathToSourceFile, int sampleRate);

        float[] ReadMonoSamplesFromStreamingUrl(string streamingUrl, int sampleRate, int secondsToDownload);

        float[] ReadMonoSamplesFromMicrophone(int sampleRate, int secondsToRecord);

        void RecodeFileToMonoWave(string pathToFile, string pathToRecodedFile, int sampleRate);

        void WriteSamplesToWaveFile(string pathToFile, float[] samples, int sampleRate);
    }
}