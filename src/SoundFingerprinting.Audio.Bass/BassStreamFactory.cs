namespace SoundFingerprinting.Audio.Bass
{
    using Un4seen.Bass;

    internal class BassStreamFactory : IBassStreamFactory
    {
        private readonly IBassServiceProxy proxy;

        public BassStreamFactory(IBassServiceProxy proxy)
        {
            this.proxy = proxy;
        }

        public int CreateStream(string pathToFile)
        {
            int stream = proxy.CreateStream(pathToFile, GetDefaultFlags());
            ThrowIfStreamIsInvalid(stream);
            return stream;
        }

        public int CreateMixerStream(int sampleRate)
        {
            int mixerStream = proxy.CreateMixerStream(sampleRate, BassConstants.NumberOfChannels, GetDefaultFlags());
            ThrowIfStreamIsInvalid(mixerStream);
            return mixerStream;
        }

        public int CreateStreamFromStreamingUrl(string streamingUrl)
        {
            int stream = proxy.CreateStreamFromUrl(streamingUrl, GetDefaultFlags());
            ThrowIfStreamIsInvalid(stream);
            return stream;
        }

        public int CreateStreamFromMicrophone(int sampleRate)
        {
            int stream = proxy.StartRecording(sampleRate, BassConstants.NumberOfChannels, BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            ThrowIfStreamIsInvalid(stream);
            return stream;
        }

        private BASSFlag GetDefaultFlags()
        {
            return BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT;
        }

        private bool IsStreamValid(int stream)
        {
            return stream != 0;
        }

        private void ThrowIfStreamIsInvalid(int stream)
        {
            if (!IsStreamValid(stream))
            {
                throw new BassAudioServiceException(proxy.GetLastError());
            }
        }
    }
}
