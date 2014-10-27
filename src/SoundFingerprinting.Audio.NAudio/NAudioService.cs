namespace SoundFingerprinting.Audio.NAudio
{
    using System.Collections.Generic;

    using SoundFingerprinting.Infrastructure;

    public class NAudioService : IAudioService
    {
        private static readonly IReadOnlyCollection<string> NAudioSupportedFormats = new[] { ".mp3", ".wav" };

        private readonly INAudioSourceReader reader;

        public NAudioService()
            : this(DependencyResolver.Current.Get<INAudioSourceReader>())
        {
            // no op
        }

        internal NAudioService(INAudioSourceReader reader)
        {
            this.reader = reader;
        }

        public IReadOnlyCollection<string> SupportedFormats
        {
            get
            {
                return NAudioSupportedFormats;
            }
        }

        public float[] ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate)
        {
            return ReadMonoSamplesFromFile(pathToSourceFile, sampleRate, seconds: 0, startAt: 0);
        }

        public float[] ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, int seconds, int startAt)
        {
            return reader.ReadMonoFromSource(pathToSourceFile, sampleRate, seconds, startAt);
        }
    }
}
