namespace SoundFingerprinting.Audio
{
    using System;
    using System.Collections.Generic;

    public abstract class AudioService : IAudioService
    {
        public const int DefaultSampleRate = 44100;

        public const int DefaultBufferLengthInSeconds = 20;

        public abstract IReadOnlyCollection<string> SupportedFormats { get; }

        public abstract float[] ReadMonoFromFile(string pathToSourceFile, int sampleRate, int seconds, int startAt);

        public float[] ReadMonoFromFile(string pathToSourceFile, int sampleRate)
        {
            return ReadMonoFromFile(pathToSourceFile, sampleRate, 0, 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool isDisposing);
    }
}