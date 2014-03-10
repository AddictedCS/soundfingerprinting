namespace SoundFingerprinting.Audio.NAudio
{
    using global::NAudio.Wave.SampleProviders;

    public class NAudioSamplesProvider : ISamplesProvider
    {
        private readonly SampleProviderConverterBase samplesProvider;

        public NAudioSamplesProvider(SampleProviderConverterBase samplesProvider)
        {
            this.samplesProvider = samplesProvider;
        }

        public int GetNextSamples(float[] buffer)
        {
            return samplesProvider.Read(buffer, 0, buffer.Length) * 4;
        }
    }
}
