namespace SoundFingerprinting.Audio.Bass
{
    using System.Collections.Generic;

    using SoundFingerprinting.Infrastructure;

    using Un4seen.Bass.Misc;

    /// <summary>
    ///   Bass Audio Service
    /// </summary>
    /// <remarks>
    ///   BASS is an audio library for use in Windows and Mac OSX software. 
    ///   Its purpose is to provide developers with powerful and efficient sample, stream (MP3, MP2, MP1, OGG, WAV, AIFF, custom generated, and more via add-ons), 
    ///   MOD music (XM, IT, S3M, MOD, MTM, UMX), MO3 music (MP3/OGG compressed MODs), and recording functions. 
    /// </remarks>
    public class BassAudioService : IAudioService
    {
        private static readonly IReadOnlyCollection<string> BaasSupportedFormats = new[] { ".wav", "mp3", ".ogg", ".flac" };

        private readonly IBassServiceProxy proxy;

        private readonly IBassStreamFactory streamFactory;

        private readonly IBassResampler bassResampler;

        public BassAudioService()
            : this(DependencyResolver.Current.Get<IBassServiceProxy>(), DependencyResolver.Current.Get<IBassStreamFactory>(), DependencyResolver.Current.Get<IBassResampler>())
        {
        }

        internal BassAudioService(IBassServiceProxy proxy, IBassStreamFactory streamFactory, IBassResampler bassResampler)
        {
            this.proxy = proxy;
            this.streamFactory = streamFactory;
            this.bassResampler = bassResampler;
        }

        public bool IsRecordingSupported
        {
            get
            {
                return proxy.GetRecordingDevice() != -1;
            }
        }

        public IReadOnlyCollection<string> SupportedFormats
        {
            get
            {
                return BaasSupportedFormats;
            }
        }

        public float[] ReadMonoFromFile(string pathToSourceFile, int sampleRate)
        {
            return ReadMonoFromFile(pathToSourceFile, sampleRate, 0, 0);
        }

        public float[] ReadMonoFromFile(string pathToSourceFile, int sampleRate, int seconds, int startAt)
        {
            int stream = streamFactory.CreateStream(pathToSourceFile);
            return bassResampler.Resample(stream, sampleRate, seconds, startAt, mixerStream => new BassSamplesProvider(proxy, mixerStream));
        }

        public float[] ReadMonoSamplesFromStreamingUrl(string streamingUrl, int sampleRate, int secondsToDownload)
        {
            int stream = streamFactory.CreateStreamFromStreamingUrl(streamingUrl);
            return bassResampler.Resample(stream, sampleRate, secondsToDownload, 0, mixerStream => new ContinuousStreamSamplesProvider(new BassSamplesProvider(proxy, mixerStream)));
        }

        public float[] ReadMonoSamplesFromMicrophone(int sampleRate, int secondsToRecord)
        {
            int stream = streamFactory.CreateStreamFromMicrophone(sampleRate);
            return bassResampler.Resample(stream, sampleRate, secondsToRecord, 0, mixerStream => new ContinuousStreamSamplesProvider(new BassSamplesProvider(proxy, mixerStream)));
        }

        public void RecodeFileToMonoWave(string pathToFile, string pathToRecodedFile, int sampleRate)
        {
            float[] samples = ReadMonoFromFile(pathToFile, sampleRate);
            WriteSamplesToWaveFile(pathToRecodedFile, samples, sampleRate);
        }

        public void WriteSamplesToWaveFile(string pathToFile, float[] samples, int sampleRate)
        {
            const int BitsPerSample = 4 * 8;
            var waveWriter = new WaveWriter(pathToFile, BassConstants.NumberOfChannels, sampleRate, BitsPerSample, true);
            waveWriter.Write(samples, samples.Length * 4);
            waveWriter.Close();
        }
    }
}
