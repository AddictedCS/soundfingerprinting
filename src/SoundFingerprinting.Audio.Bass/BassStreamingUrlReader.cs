namespace SoundFingerprinting.Audio.Bass
{
    using SoundFingerprinting.Infrastructure;

    public class BassStreamingUrlReader : IStreamingUrlReader
    {
        private readonly IBassServiceProxy proxy;
        private readonly IBassStreamFactory streamFactory;
        private readonly IBassResampler bassResampler;

        public BassStreamingUrlReader()
            : this(DependencyResolver.Current.Get<IBassServiceProxy>(), DependencyResolver.Current.Get<IBassStreamFactory>(), DependencyResolver.Current.Get<IBassResampler>())
        {
        }

        internal BassStreamingUrlReader(IBassServiceProxy proxy, IBassStreamFactory streamFactory, IBassResampler bassResampler)
        {
            this.proxy = proxy;
            this.streamFactory = streamFactory;
            this.bassResampler = bassResampler;
        }

        public float[] ReadMonoSamples(string url, int sampleRate, int secondsToRead)
        {
            int stream = streamFactory.CreateStreamFromStreamingUrl(url);
            return bassResampler.Resample(stream, sampleRate, secondsToRead, 0, mixerStream => new ContinuousStreamSamplesProvider(new BassSamplesProvider(proxy, mixerStream)));
        }
    }
}