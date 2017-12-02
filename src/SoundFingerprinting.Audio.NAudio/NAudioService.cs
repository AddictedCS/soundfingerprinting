namespace SoundFingerprinting.Audio.NAudio
{
    using System.Collections.Generic;

    using global::NAudio.Wave;

    public class NAudioService : AudioService
    {
        private static readonly IReadOnlyCollection<string> NAudioSupportedFormats = new[] { ".mp3", ".wav" };

        private readonly INAudioSourceReader sourceReader;
        private readonly int resamplerQuality;

        private readonly bool normalizeSamples;

        private readonly IAudioSamplesNormalizer audioSamplesNormalizer;

        public NAudioService(int resamplerQuality = 25, bool normalizeSamples = false)
            : this(
                resamplerQuality,
                normalizeSamples,
                new AudioSamplesNormalizer(),
                new NAudioSourceReader(new SamplesAggregator(), new NAudioFactory()))
        {
            // no op
        }

        internal NAudioService(int resamplerQuality, 
            bool normalizeSamples, 
            IAudioSamplesNormalizer audioSamplesNormalizer, 
            INAudioSourceReader sourceReader)
        {
            this.sourceReader = sourceReader;
            this.resamplerQuality = resamplerQuality;
            this.normalizeSamples = normalizeSamples;
            this.audioSamplesNormalizer = audioSamplesNormalizer;
        }

        public override float GetLengthInSeconds(string pathToSourceFile)
        {
            using (var mediaFoundationReader = new MediaFoundationReader(pathToSourceFile))
            {
                return (float)mediaFoundationReader.TotalTime.TotalSeconds;
            }
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
            if (normalizeSamples)
            {
                audioSamplesNormalizer.NormalizeInPlace(samples);    
            }

            return new AudioSamples(samples, pathToSourceFile, sampleRate);
        }
    }
}
