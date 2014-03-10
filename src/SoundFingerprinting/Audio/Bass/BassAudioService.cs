namespace SoundFingerprinting.Audio.Bass
{
    using System.Collections.Generic;
    using System.Diagnostics;

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
    public class BassAudioService : AudioService, IExtendedAudioService
    {
        private static readonly IReadOnlyCollection<string> BaasSupportedFormats = new[] { ".wav", "mp3", ".ogg", ".flac" };

        private readonly IBassServiceProxy proxy;

        private bool alreadyDisposed;
  
        public BassAudioService() : this(DependencyResolver.Current.Get<IBassServiceProxy>())
        {
        }

        private BassAudioService(IBassServiceProxy proxy)
        {
            this.proxy = proxy;
        }

        ~BassAudioService()
        {
            Dispose(false);
        }

        public bool IsRecordingSupported
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
            int stream = CreateStream(pathToSourceFile, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            return new BassResampler(proxy)
                      .Resample(stream, sampleRate, seconds, startAt, mixer => new BassSamplesProvider(proxy, mixer));
        }

        public float[] ReadMonoFromUrlToFile(string streamUrl, string pathToFile, int sampleRate, int secondsToDownload)
        {
            int stream = CreateStreamToUrl(streamUrl);

            var samples = new BassResampler(proxy).Resample(
                stream,
                sampleRate,
                secondsToDownload,
                0,
                mixer => new ContinuousStreamSamplesProvider(new BassSamplesProvider(proxy, mixer)));

            WriteSamplesToWavFile(pathToFile, sampleRate, 1, samples);
            return samples;
        }

        public float[] ReadMonoFromMicrophoneToFile(string pathToFile, int sampleRate, int secondsToRecord)
        {
            var samples = ReadFromMicrophone(sampleRate, secondsToRecord);
            WriteSamplesToWavFile(pathToFile, sampleRate, 1, samples);
            return samples;
        }

        public int PlayFile(string filename)
        {
            int stream = CreateStream(filename, BASSFlag.BASS_DEFAULT);

            if (proxy.StartPlaying(stream))
            {
                throw new BassAudioServiceException(proxy.GetLastError());
            }

            return stream;
        }

        public void StopPlayingFile(int stream)
        {
            ReleaseStream(stream);
        }

        public void RecodeFileToMonoWave(string pathToFile, string pathToRecodedFile, int sampleRate)
        {
            float[] samples = ReadMonoFromFile(pathToFile, sampleRate);
            WriteSamplesToWavFile(pathToRecodedFile, sampleRate, 1, samples);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (!alreadyDisposed)
            {
                alreadyDisposed = true;
                proxy.Dispose();
            }
        }

        private void NotifyErrorWhenReleasingMemoryStream(int stream)
        {
            Trace.WriteLine(
                "Could not release stream " + stream + ". Possible memory leak! Bass Error: " + proxy.GetLastError(),
                "Error");
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
            int stream = proxy.CreateStreamFromUrl(
                urlToResource, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);

            if (stream == 0)
            {
                throw new BassAudioServiceException(proxy.GetLastError());
            }

            return stream;
        }

        private int CreateStreamByStartingToRecord(int sampleRate)
        {
            const int NumberOfChannels = 1;
            int stream = proxy.StartRecording(
                sampleRate, NumberOfChannels, BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);

            if (stream == 0)
            {
                throw new BassAudioServiceException(proxy.GetLastError());
            }

            return stream;
        }

        private void ReleaseStream(int stream)
        {
            if (stream != 0 && !proxy.FreeStream(stream))
            {
                NotifyErrorWhenReleasingMemoryStream(stream);
            }
        }

        private void WriteSamplesToWavFile(string pathToFile, int sampleRate, int channels, float[] samples)
        {
            WaveWriter waveWriter = new WaveWriter(pathToFile, channels, sampleRate, 8 * 4, true);
            waveWriter.Write(samples, samples.Length * 4);
            waveWriter.Close();
        }

        private float[] ReadFromMicrophone(int sampleRate, int secondsToRecord)
        {
            int stream = CreateStreamByStartingToRecord(sampleRate);
            return new BassResampler(proxy).Resample(
                stream,
                sampleRate,
                secondsToRecord,
                0,
                mixer => new ContinuousStreamSamplesProvider(new BassSamplesProvider(proxy, mixer)));
        }
    }
}
