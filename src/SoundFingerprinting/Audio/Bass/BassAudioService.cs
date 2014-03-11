namespace SoundFingerprinting.Audio.Bass
{
    using System.Collections.Generic;

    using SoundFingerprinting.Infrastructure;

    using Un4seen.Bass;
    using Un4seen.Bass.Misc;

    /// <summary>
    ///   Bass Audio Service
    /// </summary>
    /// <remarks>
    ///   BASS is an audio library for use in Windows and Mac OSX software. 
    ///   Its purpose is to provide developers with powerful and efficient sample, stream (MP3, MP2, MP1, OGG, WAV, AIFF, custom generated, and more via add-ons), 
    ///   MOD music (XM, IT, S3M, MOD, MTM, UMX), MO3 music (MP3/OGG compressed MODs), and recording functions. 
    /// </remarks>
    public class BassAudioService : AudioService
    {
        private const int NumberOfChannels = 1;
            
        private static readonly IReadOnlyCollection<string> BaasSupportedFormats = new[] { ".wav", "mp3", ".ogg", ".flac" };

        private readonly IBassServiceProxy proxy;

        private readonly BassResampler bassResampler;

        public BassAudioService() : this(DependencyResolver.Current.Get<IBassServiceProxy>())
        {
        }

        private BassAudioService(IBassServiceProxy proxy)
        {
            this.proxy = proxy;
            bassResampler = new BassResampler(proxy);
        }

        public override bool IsRecordingSupported
        {
            get
            {
                return proxy.GetRecordingDevice() != -1;
            }
        }

        public override IReadOnlyCollection<string> SupportedFormats
        {
            get
            {
                return BaasSupportedFormats;
            }
        }

        public override float[] ReadMonoFromFile(string pathToSourceFile, int sampleRate, int seconds, int startAt)
        {
            int stream = CreateStream(pathToSourceFile, GetDefaultFlags());
            return bassResampler.Resample(stream, sampleRate, seconds, startAt, mixer => new BassSamplesProvider(proxy, mixer));
        }

        public override float[] ReadMonoSamplesFromStreamingUrl(string streamUrl, int sampleRate, int secondsToDownload)
        {
            int stream = CreateStreamToUrl(streamUrl);
            return bassResampler.Resample(stream, sampleRate, secondsToDownload, 0, mixer => new ContinuousStreamSamplesProvider(new BassSamplesProvider(proxy, mixer)));
        }

        public override float[] ReadMonoSamplesFromMicrophone(int sampleRate, int secondsToRecord)
        {
            int stream = CreateStreamByStartingToRecord(sampleRate);
            return bassResampler.Resample(stream, sampleRate, secondsToRecord, 0, mixer => new ContinuousStreamSamplesProvider(new BassSamplesProvider(proxy, mixer)));
        }

        public override void RecodeFileToMonoWave(string pathToFile, string pathToRecodedFile, int sampleRate)
        {
            float[] samples = ReadMonoFromFile(pathToFile, sampleRate);
            WriteSamplesToWaveFile(pathToRecodedFile, samples, sampleRate);
        }

        public override void WriteSamplesToWaveFile(string pathToFile, float[] samples, int sampleRate)
        {
            const int BitsPerSample = 4 * 8;
            var waveWriter = new WaveWriter(pathToFile, NumberOfChannels, sampleRate, BitsPerSample, true);
            waveWriter.Write(samples, samples.Length * 4);
            waveWriter.Close();
        }

        private int CreateStream(string pathToFile, BASSFlag flags)
        {
            // create streams for re-sampling
            int stream = proxy.CreateStream(pathToFile, flags);

            if (stream == 0)
            {
                throw new BassAudioServiceException(proxy.GetLastError());
            }

            return stream;
        }

        private int CreateStreamToUrl(string urlToResource)
        {
            int stream = proxy.CreateStreamFromUrl(urlToResource, GetDefaultFlags());

            if (stream == 0)
            {
                throw new BassAudioServiceException(proxy.GetLastError());
            }

            return stream;
        }

        private int CreateStreamByStartingToRecord(int sampleRate)
        {
            int stream = proxy.StartRecording(sampleRate, NumberOfChannels, BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);

            if (stream == 0)
            {
                throw new BassAudioServiceException(proxy.GetLastError());
            }

            return stream;
        }

        private BASSFlag GetDefaultFlags()
        {
            return BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT;
        }
    }
}
