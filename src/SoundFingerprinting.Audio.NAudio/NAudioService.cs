namespace SoundFingerprinting.Audio.NAudio
{
    using System.Collections.Generic;

    using SoundFingerprinting.Infrastructure;

    public class NAudioService : AudioService
    {
        private static readonly IReadOnlyCollection<string> NAudioSupportedFormats = new[] { ".mp3", ".wav" };

        private readonly INAudioSourceReader sourceReader;

        public NAudioService()
            : this(DependencyResolver.Current.Get<INAudioSourceReader>())
        {
            // no op
        }

        internal NAudioService(INAudioSourceReader sourceReader)
        {
            this.sourceReader = sourceReader;
        }

        public override IReadOnlyCollection<string> SupportedFormats
        {
            get
            {
                return NAudioSupportedFormats;
            }
        }

        public override AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, int seconds, int startAt)
        {
            float[] samples = sourceReader.ReadMonoFromSource(pathToSourceFile, sampleRate, seconds, startAt);
            return new AudioSamples
                {
                    Samples = samples,
                    Origin = pathToSourceFile,
                    SampleRate = sampleRate,
                    Duration = (double)samples.Length / sampleRate
                };
        }
    }
}
