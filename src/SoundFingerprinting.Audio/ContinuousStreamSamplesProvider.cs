namespace SoundFingerprinting.Audio
{
    using System.Threading;

    public class ContinuousStreamSamplesProvider : ISamplesProvider
    {
        private const int MillisecondsTimeout = 500;

        private readonly ISamplesProvider provider;

        public ContinuousStreamSamplesProvider(ISamplesProvider provider)
        {
            this.provider = provider;
        }

        public int GetNextSamples(float[] buffer)
        {
            int bytesRead = provider.GetNextSamples(buffer);

            while (bytesRead == 0)
            {
                Thread.Sleep(MillisecondsTimeout); // lame but required to fill the buffer from continuous stream, either microphone or url
                bytesRead = provider.GetNextSamples(buffer);
            }

            return bytesRead;
        }
    }
}