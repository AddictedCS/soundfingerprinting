namespace SoundFingerprinting.Audio.Bass
{
    internal class BassSamplesProvider : ISamplesProvider
    {
        private readonly IBassServiceProxy proxy;

        private readonly int source;

        public BassSamplesProvider(IBassServiceProxy proxy, int source)
        {
            this.proxy = proxy;
            this.source = source;
        }

        public int GetNextSamples(float[] buffer)
        {
            return proxy.ChannelGetData(source, buffer, buffer.Length * 4);
        }
    }
}
