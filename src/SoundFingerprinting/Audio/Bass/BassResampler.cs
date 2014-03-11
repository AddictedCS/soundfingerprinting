namespace SoundFingerprinting.Audio.Bass
{
    using System;

    using Un4seen.Bass;

    internal class BassResampler : IBassResampler
    {
        private readonly IBassServiceProxy proxy;

        private readonly IBassStreamFactory streamFactory;

        private readonly ISamplesAggregator samplesAggregator;

        public BassResampler(IBassServiceProxy proxy, IBassStreamFactory streamFactory, ISamplesAggregator samplesAggregator)
        {
            this.proxy = proxy;
            this.streamFactory = streamFactory;
            this.samplesAggregator = samplesAggregator;
        }

        public float[] Resample(int sourceStream, int sampleRate, int seconds, int startAt, Func<int, ISamplesProvider> getSamplesProvider)
        {
            int mixerStream = 0;
            try
            {
                SeekToSecondInCaseIfRequired(sourceStream, startAt);
                mixerStream = streamFactory.CreateMixerStream(sampleRate);
                CombineStreams(mixerStream, sourceStream);
                return samplesAggregator.ReadSamplesFromSource(getSamplesProvider(mixerStream), seconds, sampleRate);
            }
            finally
            {
                ReleaseStream(mixerStream);
                ReleaseStream(sourceStream);
            }
        }

        private void SeekToSecondInCaseIfRequired(int stream, int startAtSecond)
        {
            if (startAtSecond > 0)
            {
                if (!proxy.ChannelSetPosition(stream, startAtSecond))
                {
                    throw new BassAudioServiceException(proxy.GetLastError());
                }
            }
        }

        private void CombineStreams(int mixerStream, int stream)
        {
            if (!proxy.CombineMixerStreams(mixerStream, stream, BASSFlag.BASS_MIXER_FILTER))
            {
                throw new BassAudioServiceException(proxy.GetLastError());
            }
        }

        private void ReleaseStream(int stream)
        {
            if (stream != 0)
            {
                proxy.FreeStream(stream);
            }
        }
    }
}
