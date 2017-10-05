namespace SoundFingerprinting.Audio.NAudio
{
    using System.Collections.Generic;

    using SoundFingerprinting.Infrastructure;

    public class NAudioService : AudioService
    {
        private static readonly IReadOnlyCollection<string> NAudioSupportedFormats = new[] { ".mp3", ".wav" };

        private readonly INAudioSourceReader sourceReader;
        private readonly int resamplerQuality;

        public NAudioService(int resamplerQuality = 25)
            : this(resamplerQuality, DependencyResolver.Current.Get<INAudioSourceReader>())
        {
            // no op
        }

        internal NAudioService(int resamplerQuality, INAudioSourceReader sourceReader)
        {
            this.sourceReader = sourceReader;
            this.resamplerQuality = resamplerQuality;
        }

        public override IReadOnlyCollection<string> SupportedFormats
        {
            get
            {
                return NAudioSupportedFormats;
            }
        }

        public override AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, double seconds, double startAt)
        {
            float[] samples = sourceReader.ReadMonoFromSource(pathToSourceFile, sampleRate, seconds, startAt, resamplerQuality);
            return new AudioSamples(samples, pathToSourceFile, sampleRate);
        }
    }
}
