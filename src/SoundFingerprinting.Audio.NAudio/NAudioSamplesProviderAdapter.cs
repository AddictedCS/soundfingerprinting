namespace SoundFingerprinting.Audio.NAudio
{
    using global::NAudio.Wave;

    public class NAudioSamplesProviderAdapter : ISamplesProvider
    {
        private readonly ISampleProvider samplesProvider;

        public NAudioSamplesProviderAdapter(ISampleProvider samplesProvider)
        {
            this.samplesProvider = samplesProvider;
        }

        public int GetNextSamples(float[] buffer)
        {
            return samplesProvider.Read(buffer, 0, buffer.Length) * sizeof(float);
        }
    }
}
