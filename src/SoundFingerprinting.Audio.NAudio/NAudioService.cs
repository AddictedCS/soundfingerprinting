namespace SoundFingerprinting.Audio.NAudio
{
    using System.Collections.Generic;

    using global::NAudio.Wave;

    public class NAudioService : AudioService
    {
        private static readonly IReadOnlyCollection<string> NAudioSupportedFormats = new[] { ".mp3", ".wav" };

        private readonly INAudioSourceReader sourceReader;
        private readonly int downSamplingQuality;

        public NAudioService(int downSamplingQuality = 25) : this(downSamplingQuality, new NAudioSourceReader(new SamplesAggregator(), new NAudioFactory()))
        {
            // no op
        }

        internal NAudioService(int downSamplingQuality, INAudioSourceReader sourceReader)
        {
            this.sourceReader = sourceReader;
            this.downSamplingQuality = downSamplingQuality;
        }

        public override float GetLengthInSeconds(string file)
        {
            using (var mediaFoundationReader = new MediaFoundationReader(file))
            {
                return (float)mediaFoundationReader.TotalTime.TotalSeconds;
            }
        }

        public override IReadOnlyCollection<string> SupportedFormats => NAudioSupportedFormats;

        public override AudioSamples ReadMonoSamplesFromFile(string pathToSourceFile, int sampleRate, double seconds, double startAt)
        {
            var samples = sourceReader.ReadMonoFromSource(pathToSourceFile, sampleRate, seconds, startAt, downSamplingQuality);
            return new AudioSamples(samples, pathToSourceFile, sampleRate);
        }
    }
}
