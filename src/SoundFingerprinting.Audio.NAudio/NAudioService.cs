namespace SoundFingerprinting.Audio.NAudio
{
    using System.Collections.Generic;

    public class NAudioService : AudioService
    {
        private static readonly IReadOnlyCollection<string> NAudioSupportedFormats = new[] { ".mp3", ".wav" };

        private readonly INAudioSourceReader sourceReader;

        public NAudioService(int resamplerQuality = 25) : this(new NAudioSourceReader(resamplerQuality))
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

        public override AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, double seconds, double startAt)
        {
            float[] samples = sourceReader.ReadMonoFromSource(pathToSourceFile, sampleRate, seconds, startAt);
            return new AudioSamples(samples, pathToSourceFile, sampleRate);
        }
    }
}
