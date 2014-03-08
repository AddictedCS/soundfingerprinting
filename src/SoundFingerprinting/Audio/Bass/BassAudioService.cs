namespace SoundFingerprinting.Audio.Bass
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using SoundFingerprinting.Infrastructure;

    using Un4seen.Bass;
    using Un4seen.Bass.AddOn.Tags;
    using Un4seen.Bass.Misc;

    /// <summary>
    ///   Bass Audio Service
    /// </summary>
    /// <remarks>
    ///   BASS is an audio library for use in Windows and Mac OSX software. 
    ///   Its purpose is to provide developers with powerful and efficient sample, stream (MP3, MP2, MP1, OGG, WAV, AIFF, custom generated, and more via add-ons), 
    ///   MOD music (XM, IT, S3M, MOD, MTM, UMX), MO3 music (MP3/OGG compressed MODs), and recording functions. 
    /// </remarks>
    public class BassAudioService : AudioService, IExtendedAudioService, ITagService
    {
        private const string FlacDllName = "bassflac.dll";

        private static readonly IReadOnlyCollection<string> BaasSupportedFormats = new[] { ".wav", "mp3", ".ogg", ".flac" };

        private static readonly object LockObject = new object();

        private static int initializedInstances;

        private readonly IBassServiceProxy bassServiceProxy;

        private readonly SamplesAggregator samplesAggregator;

        private bool alreadyDisposed;
  
        public BassAudioService() : this(DependencyResolver.Current.Get<IBassServiceProxy>())
        {
            samplesAggregator = new SamplesAggregator();
        }

        private BassAudioService(IBassServiceProxy bassServiceProxy)
        {
            this.bassServiceProxy = bassServiceProxy;
            lock (LockObject)
            {
                if (!IsNativeBassLibraryInitialized)
                {
                    RegisterBassKey();
                    
                    string targetPath = GetTargetPathToLoadLibrariesFrom();

                    LoadBassLibraries(targetPath);

                    CheckIfFlacPluginIsLoaded(targetPath);

                    InitializeBassLibraryWithAudioDevices();

                    SetDefaultConfigs();

                    InitializeRecordingDevice();
                }

                initializedInstances++;
            }
        }

        ~BassAudioService()
        {
            Dispose(false);
        }

        public static bool IsNativeBassLibraryInitialized
        {
            get
            {
                return initializedInstances != 0;
            }
        }

        public bool IsRecordingSupported
        {
            get
            {
                return bassServiceProxy.GetRecordingDevice() != -1;
            }
        }

        public override IReadOnlyCollection<string> SupportedFormats
        {
            get
            {
                return BaasSupportedFormats;
            }
        }

        public override float[] ReadMonoFromFile(string pathToFile, int sampleRate, int secondsToRead, int startAtSecond)
        {
            int stream = CreateStream(pathToFile, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            return DownsampleStreamWithMixer(stream, sampleRate, secondsToRead, startAtSecond);
        }

        public float[] ReadMonoFromUrl(string urlToResource, int sampleRate, int secondsToDownload)
        {
            int stream = CreateStreamToUrl(urlToResource);
            return DownsampleStreamWithMixer(stream, sampleRate, secondsToDownload, 0);
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

            if (bassServiceProxy.StartPlaying(stream))
            {
                throw new BassAudioServiceException(bassServiceProxy.GetLastError());
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

        public TagInfo GetTagInfo(string pathToAudioFile)
        {
            TAG_INFO tags = bassServiceProxy.GetTagsFromFile(pathToAudioFile);
            if (tags == null)
            {
                return new TagInfo { IsEmpty = true };
            }

            int year;
            int.TryParse(tags.year, out year);
            TagInfo tag = new TagInfo
                {
                    Duration = tags.duration,
                    Album = tags.album,
                    Artist = tags.artist,
                    Title = tags.title,
                    AlbumArtist = tags.albumartist,
                    Genre = tags.genre,
                    Year = year,
                    Composer = tags.composer,
                    ISRC = tags.isrc
                };

            return tag;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (!alreadyDisposed)
            {
                alreadyDisposed = true;

                if (isDisposing)
                {
                    // release managed resources
                }

                lock (LockObject)
                {
                    if (IsSafeToDisposeNativeBassLibrary())
                    {
                        // 0 - free all loaded plugins
                        if (!bassServiceProxy.PluginFree(0))
                        {
                            Trace.WriteLine("Could not unload plugins for Bass library.", "Error");
                        }

                        if (!bassServiceProxy.BassFree())
                        {
                            Trace.WriteLine("Could not free Bass library. Possible memory leak!", "Error");
                        }
                    }

                    initializedInstances--;
                }
            }
        }

        private bool IsSafeToDisposeNativeBassLibrary()
        {
            return initializedInstances == 1;
        }

        private void LoadBassLibraries(string targetPath)
        {
            if (!bassServiceProxy.BassLoadMe(targetPath))
            {
                throw new BassAudioServiceException("Could not load bass native libraries from the following path: " + targetPath);
            }

            if (!bassServiceProxy.BassMixLoadMe(targetPath))
            {
                throw new BassAudioServiceException("Could not load bassmix library from the following path: " + targetPath);
            }

            if (!bassServiceProxy.BassFxLoadMe(targetPath))
            {
                throw new BassAudioServiceException("Could not load bassfx library from the following path: " + targetPath);
            }
            
            DummyCallToLoadBassLibraries();
        }

        private void DummyCallToLoadBassLibraries()
        {
            bassServiceProxy.GetVersion();
            bassServiceProxy.GetMixerVersion();
            bassServiceProxy.GetFxVersion();
        }

        private void InitializeBassLibraryWithAudioDevices()
        {
            if (!bassServiceProxy.Init(-1, DefaultSampleRate, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_MONO))
            {
                Trace.WriteLine("Failed to find a sound device on running machine. Playing audio files will not be supported. " + bassServiceProxy.GetLastError(), "Warning");
                if (!bassServiceProxy.Init(0, DefaultSampleRate, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_MONO))
                {
                    throw new BassAudioServiceException(bassServiceProxy.GetLastError());
                }
            }
        }

        private void CheckIfFlacPluginIsLoaded(string targetPath)
        {
            var loadedPlugIns = bassServiceProxy.PluginLoadDirectory(targetPath);
            if (!loadedPlugIns.Any(p => p.Value.EndsWith(FlacDllName)))
            {
                Trace.WriteLine("Could not load bassflac.dll. FLAC format is not supported!", "Warning");
            }
        }

        private string GetTargetPathToLoadLibrariesFrom()
        {
            string executingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            if (string.IsNullOrEmpty(executingPath))
            {
                throw new BassAudioServiceException(
                    "Executing path of the application is null or empty. Could not find folders with native dll libraries.");
            }

            Uri uri = new Uri(executingPath);
            return Path.Combine(uri.LocalPath, Utils.Is64Bit ? "x64" : "x86");
        }

        private void SetDefaultConfigs()
        {
            /*Set filter for anti aliasing*/
            if (!bassServiceProxy.SetConfig(BASSConfig.BASS_CONFIG_MIXER_FILTER, 50))
            {
                throw new BassAudioServiceException(bassServiceProxy.GetLastError());
            }

            /*Set floating parameters to be passed*/
            if (!bassServiceProxy.SetConfig(BASSConfig.BASS_CONFIG_FLOATDSP, true))
            {
                throw new BassAudioServiceException(bassServiceProxy.GetLastError());
            }
        }

        private void InitializeRecordingDevice()
        {
            const int DefaultDevice = -1;
            if (!bassServiceProxy.RecordInit(DefaultDevice))
            {
                Trace.WriteLine(
                    "No default recording device could be found on running machine. Recording is not supported: "
                    + bassServiceProxy.GetLastError(),
                    "Warning");
            }
        }

        private void NotifyErrorWhenReleasingMemoryStream(int stream)
        {
            Trace.WriteLine(
                "Could not release stream " + stream + ". Possible memory leak! Bass Error: " + bassServiceProxy.GetLastError(),
                "Error");
        }

        private void SeekToSecondInCaseIfRequired(int stream, int startAtSecond)
        {
            if (startAtSecond > 0)
            {
                if (!bassServiceProxy.ChannelSetPosition(stream, startAtSecond))
                {
                    throw new BassAudioServiceException(bassServiceProxy.GetLastError());
                }
            }
        }

        private void CombineStreams(int mixerStream, int stream)
        {
            if (!bassServiceProxy.CombineMixerStreams(mixerStream, stream, BASSFlag.BASS_MIXER_FILTER))
            {
                throw new BassAudioServiceException(bassServiceProxy.GetLastError());
            }
        }

        private int CreateMixerStream(int sampleRate)
        {
            int mixerStream = bassServiceProxy.CreateMixerStream(
                sampleRate,
                1,
                BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);

            if (mixerStream == 0)
            {
                throw new BassAudioServiceException(bassServiceProxy.GetLastError());
            }

            return mixerStream;
        }

        private int CreateStream(string pathToFile, BASSFlag flags)
        {
            // create streams for re-sampling
            int stream = bassServiceProxy.CreateStream(pathToFile, flags);

            if (stream == 0)
            {
                throw new BassAudioServiceException(bassServiceProxy.GetLastError());
            }

            return stream;
        }

        private int CreateStreamToUrl(string urlToResource)
        {
            int stream = bassServiceProxy.CreateStreamFromUrl(
                urlToResource, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);

            if (stream == 0)
            {
                throw new BassAudioServiceException(bassServiceProxy.GetLastError());
            }

            return stream;
        }

        private int CreateStreamByStartingToRecord(int sampleRate)
        {
            int stream = bassServiceProxy.StartRecording(
                sampleRate, 1, BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);

            if (stream == 0)
            {
                throw new BassAudioServiceException(bassServiceProxy.GetLastError());
            }

            return stream;
        }

        private void ReleaseStream(int stream)
        {
            if (stream != 0 && !bassServiceProxy.FreeStream(stream))
            {
                NotifyErrorWhenReleasingMemoryStream(stream);
            }
        }

        private float[] DownsampleStreamWithMixer(int stream, int sampleRate, int secondsToRead, int startAtSecond)
        {
            int mixerStream = 0;
            try
            {
                SeekToSecondInCaseIfRequired(stream, startAtSecond);
                mixerStream = CreateMixerStream(sampleRate);
                CombineStreams(mixerStream, stream);
                return samplesAggregator.ReadSamplesFromSource(mixerStream, secondsToRead, sampleRate, GetNextSamples);
            }
            finally
            {
                ReleaseStream(mixerStream);
                ReleaseStream(stream);
            }
        }

        private int GetNextSamples(int source, float[] buffer)
        {
            return bassServiceProxy.ChannelGetData(source, buffer, buffer.Length * 4);
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
            return DownsampleStreamWithMixer(stream, sampleRate, secondsToRecord, 0);
        }

        private void RegisterBassKey()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var bassConfigurationSection = config.GetSection("BassConfigurationSection") as BassConfigurationSection;

            if (bassConfigurationSection != null)
            {
                bassServiceProxy.RegisterBass(bassConfigurationSection.Email, bassConfigurationSection.RegistrationKey); // Call to avoid the freeware splash screen. Didn't see it, but maybe it will appear if the Forms are used
            }
        }
    }
}
