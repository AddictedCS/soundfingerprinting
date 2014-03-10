namespace SoundFingerprinting.Audio.Bass
{
    using System;
    using System.Diagnostics;

    using Un4seen.Bass;

    internal class BassResampler
    {
        private readonly IBassServiceProxy proxy;

        private readonly SamplesAggregator samplesAggregator;

        internal BassResampler(IBassServiceProxy proxy)
        {
            this.proxy = proxy;
            samplesAggregator = new SamplesAggregator();
        }

        public float[] Resample(int sourceStream, int sampleRate, int seconds, int startAt, Func<int, ISamplesProvider> getSamplesProvider)
        {
            int mixerStream = 0;
            try
            {
                SeekToSecondInCaseIfRequired(sourceStream, startAt);
                mixerStream = CreateMixerStream(sampleRate);
                CombineStreams(mixerStream, sourceStream);
                return ReadSamplesFromSource(getSamplesProvider(mixerStream), seconds, sampleRate);
            }
            finally
            {
                ReleaseStream(mixerStream);
                ReleaseStream(sourceStream);
            }
        }

        protected virtual float[] ReadSamplesFromSource(ISamplesProvider samplesProvider, int seconds, int sampleRate)
        {
            return samplesAggregator.ReadSamplesFromSource(samplesProvider, seconds, sampleRate);
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

        private int CreateMixerStream(int sampleRate)
        {
            int mixerStream = proxy.CreateMixerStream(
                sampleRate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);

            if (mixerStream == 0)
            {
                throw new BassAudioServiceException(proxy.GetLastError());
            }

            return mixerStream;
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
