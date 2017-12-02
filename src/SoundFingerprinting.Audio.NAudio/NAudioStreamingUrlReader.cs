namespace SoundFingerprinting.Audio.NAudio
{
    public class NAudioStreamingUrlReader : IStreamingUrlReader
    {
        private const int DefaultResamplerQuality = 25;
        private readonly INAudioSourceReader reader;

        public NAudioStreamingUrlReader() : this(new NAudioSourceReader(new SamplesAggregator(), new NAudioFactory()))
        {
            // no op
        }

        internal NAudioStreamingUrlReader(INAudioSourceReader reader)
        {
            this.reader = reader;
        }

        public float[] ReadMonoSamples(string url, int sampleRate, int secondsToRead)
        {
            // When reading directly from URL NAudio 1.7.1 disregards Mono resampler parameter, thus reading stereo samples
            // End result has to be converted to Mono in order to comply to interface requirements
            // The issue has been addressed here: http://stackoverflow.com/questions/22385783/aac-stream-resampled-incorrectly though not yet resolved
            float[] stereoSamples = reader.ReadMonoFromSource(url, sampleRate, secondsToRead * 2 /*for stereo request twice as much data as for mono*/, startAtSecond: 0, resamplerQuality: DefaultResamplerQuality);
            return ConvertStereoSamplesToMono(stereoSamples);
        }

        private float[] ConvertStereoSamplesToMono(float[] stereoSamples)
        {
            float[] monoSamples = new float[stereoSamples.Length / 2];
            for (int i = 0; i < stereoSamples.Length; i += 2)
            {
                float sum = stereoSamples[i] + stereoSamples[i + 1];
                if (sum > short.MaxValue)
                {
                    sum = short.MaxValue;
                }

                monoSamples[i / 2] = sum / 2;
            }

            return monoSamples;
        }
    }
}