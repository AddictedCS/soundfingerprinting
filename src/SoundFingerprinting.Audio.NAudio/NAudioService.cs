namespace SoundFingerprinting.Audio.NAudio
{
    using System.Collections.Generic;

    using SoundFingerprinting.Infrastructure;

    public class NAudioService : IAudioService
    {
        private const int Mono = 1;

        private static readonly IReadOnlyCollection<string> NAudioSupportedFormats = new[] { ".mp3", ".wav" };

        private readonly INAudioFactory factory;
        private readonly INAudioSourceReader reader;

        public NAudioService()
            : this(DependencyResolver.Current.Get<INAudioSourceReader>(), DependencyResolver.Current.Get<INAudioFactory>())
        {
            // no op
        }

        internal NAudioService(INAudioSourceReader reader, INAudioFactory factory)
        {
            this.factory = factory;
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
      
        public void RecodeFileToMonoWave(string pathToFile, string pathToRecodedFile, int sampleRate)
        {
            using (var stream = factory.GetStream(pathToFile))
            {
                using (var resampler = factory.GetResampler(stream, sampleRate, Mono))
                {
                    factory.CreateWaveFile(pathToRecodedFile, resampler);
                }
            }
        }
    }
}
