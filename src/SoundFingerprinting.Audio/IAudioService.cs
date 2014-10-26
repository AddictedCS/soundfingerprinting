namespace SoundFingerprinting.Audio
{
    using System.Collections.Generic;

    public interface IAudioService
    {
        bool IsRecordingSupported { get; }

        IReadOnlyCollection<string> SupportedFormats { get; }

        float[] ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, int seconds, int startAt);

        float[] ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate);

        float[] ReadMonoSamplesFromStreamingUrl(string streamingUrl, int sampleRate, int secondsToDownload);

        float[] ReadMonoSamplesFromMicrophone(int sampleRate, int secondsToRecord);
    }
}