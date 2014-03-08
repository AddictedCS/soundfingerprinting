namespace SoundFingerprinting.Audio.NAudio
{
    using System;
    using System.Collections.Generic;

    using global::NAudio.Wave;
    using global::NAudio.Wave.SampleProviders;

    public class NAudioService : AudioService, IExtendedAudioService
    {
        private static readonly IReadOnlyCollection<string> NAudioSupportedFormats = new[] { ".mp3", ".wav" };

        private IWavePlayer waveOutDevice;
        private WaveStream mainOutputStream;

        ~NAudioService()
        {
            Dispose(false);
        }

        public bool IsRecordingSupported
        {
            get
            {
                return false;
            }
        }

        public override IReadOnlyCollection<string> SupportedFormats
        {
            get
            {
                return NAudioSupportedFormats;
            }
        }

        public override float[] ReadMonoFromFile(string pathToFile, int sampleRate, int secondsToRead, int startAtSecond)
        {
            using (var reader = new MediaFoundationReader(pathToFile))
            {
                SeekToSecondInCaseIfRequired(startAtSecond, reader);
                using (
                    var resampler = new MediaFoundationResampler(
                        reader, WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1)))
                {
                    var pcmReader = new Pcm32BitToSampleProvider(resampler);
                    return ReadSamplesFromSource(pcmReader, secondsToRead, sampleRate);
                }
            }
        }

        public int PlayFile(string filename)
        {
            waveOutDevice = new WaveOut();
            mainOutputStream = CreateInputStream(filename);
            waveOutDevice.Init(mainOutputStream);
            waveOutDevice.Play();
            return waveOutDevice.GetHashCode();
        }

        public void StopPlayingFile(int stream)
        {
            if (waveOutDevice != null)
            {
                waveOutDevice.Stop();
            }

            if (mainOutputStream != null)
            {
                mainOutputStream.Close();
                mainOutputStream = null;
            }

            if (waveOutDevice != null)
            {
                waveOutDevice.Dispose();
                waveOutDevice = null;
            }
        }

        public void RecodeFileToMonoWave(string pathToFile, string pathToRecodedFile, int sampleRate)
        {
            using (var reader = new Mp3FileReader(pathToFile))
            {
                using (var resampler = new MediaFoundationResampler(reader, WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1)))
                {
                    WaveFileWriter.CreateWaveFile(pathToRecodedFile, resampler);
                }
            }
        }

        public float[] ReadMonoFromUrl(string urlToResource, int sampleRate, int secondsToDownload)
        {
            throw new NotImplementedException("Use Bass.NET");
        }

        public float[] ReadMonoFromMicrophoneToFile(string pathToFile, int sampleRate, int secondsToRecord)
        {
            throw new NotImplementedException("Use Bass.NET");
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (waveOutDevice != null)
                {
                    waveOutDevice.Dispose();
                }

                if (mainOutputStream != null)
                {
                    mainOutputStream.Dispose();
                }
            }
        }

        protected override int ReadNextSamples(object source, float[] buffer)
        {
            return ((Pcm32BitToSampleProvider)source).Read(buffer, 0, buffer.Length) * 4;
        }

        private WaveStream CreateInputStream(string fileName)
        {
            var mp3Reader = new Mp3FileReader(fileName);
            return new WaveChannel32(mp3Reader);
        }

        private void SeekToSecondInCaseIfRequired(int startAtSecond, MediaFoundationReader reader)
        {
            if (startAtSecond > 0)
            {
                int actualSampleRate = reader.WaveFormat.SampleRate;
                int bitsPerSample = reader.WaveFormat.BitsPerSample;
                reader.Seek(actualSampleRate * bitsPerSample / 8 * startAtSecond, System.IO.SeekOrigin.Begin);
            }
        }
    }
}
