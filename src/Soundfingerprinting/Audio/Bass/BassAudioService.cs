namespace Soundfingerprinting.Audio.Bass
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Un4seen.Bass;
    using Un4seen.Bass.AddOn.Fx;
    using Un4seen.Bass.AddOn.Mix;
    using Un4seen.Bass.AddOn.Tags;
    using Un4seen.Bass.Misc;

    /// <summary>
    ///   Bass Proxy for Bass.Net API
    /// </summary>
    /// <remarks>
    ///   BASS is an audio library for use in Windows and Mac OSX software. 
    ///   Its purpose is to provide developers with powerful and efficient sample, stream (MP3, MP2, MP1, OGG, WAV, AIFF, custom generated, and more via add-ons), 
    ///   MOD music (XM, IT, S3M, MOD, MTM, UMX), MO3 music (MP3/OGG compressed MODs), and recording functions. 
    ///   All in a tiny DLL, under 100KB* in size.
    /// </remarks>
    public class BassAudioService : AudioService, IExtendedAudioService, ITagService
    {
        private const int DefaultSampleRate = 44100;

        private bool alreadyDisposed;

        static BassAudioService()
        {
            string targetPath = Path.Combine(Environment.CurrentDirectory, Utils.Is64Bit ? "x64" : "x86");

            // Call to avoid the freeware splash screen. Didn't see it, but maybe it will appear if the Forms are used :D
            BassNet.Registration("gleb.godonoga@gmail.com", "2X155323152222");

            // Dummy calls made for loading the assemblies
#pragma warning disable 168
            bool isBassLoad = Bass.LoadMe(targetPath);
            bool isBassMixLoad = BassMix.LoadMe(targetPath);
            bool isBassFxLoad = BassFx.LoadMe(targetPath);
            int bassVersion = Bass.BASS_GetVersion();
            int bassMixVersion = BassMix.BASS_Mixer_GetVersion();
            int bassfxVersion = BassFx.BASS_FX_GetVersion();
#pragma warning restore 168
            var loadedPlugIns = Bass.BASS_PluginLoadDirectory(targetPath);
            if (!loadedPlugIns.Any(p => p.Value.EndsWith("bassflac.dll")))
            {
                throw new Exception("Couldnt load the bass flac plugin!");
            }

            // Set Sample Rate / MONO
            if (!Bass.BASS_Init(-1, DefaultSampleRate, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_MONO, IntPtr.Zero))
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            /*Set filter for anti aliasing*/
            if (!Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_MIXER_FILTER, 50))
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            /*Set floating parameters to be passed*/
            if (!Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_FLOATDSP, true))
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            // use default device
            if (!Bass.BASS_RecordInit(-1))
            {
                Debug.WriteLine("No recording device could be found on running machine. Recording is not supported: " + Bass.BASS_ErrorGetCode().ToString());
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BassAudioService"/> class.
        /// </summary>
        ~BassAudioService()
        {
            Dispose(true);
        }

        public bool IsRecordingSupported
        {
            get
            {
                return Bass.BASS_RecordGetDevice() != -1;
            }
        }

        /// <summary>
        ///   Read mono from file
        /// </summary>
        /// <param name = "pathToFile">Name of the file</param>
        /// <param name = "sampleRate">Output sample rate</param>
        /// <param name = "secondsToRead">Milliseconds to read</param>
        /// <param name = "startAtSecond">Start millisecond</param>
        /// <returns>Array of samples</returns>
        /// <remarks>
        ///   Seeking capabilities of Bass where not used because of the possible
        ///   timing errors on different formats.
        /// </remarks>
        public override float[] ReadMonoFromFile(string pathToFile, int sampleRate, int secondsToRead, int startAtSecond)
        {
            // create streams for re-sampling
            int stream = Bass.BASS_StreamCreateFile(pathToFile, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT); // Decode the stream

            if (stream == 0)
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            int mixerStream = BassMix.BASS_Mixer_StreamCreate(sampleRate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            if (mixerStream == 0)
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            if (!BassMix.BASS_Mixer_StreamAddChannel(mixerStream, stream, BASSFlag.BASS_MIXER_FILTER))
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            if (startAtSecond > 0)
            {
                if (!Bass.BASS_ChannelSetPosition(stream, startAtSecond))
                {
                    throw new Exception(Bass.BASS_ErrorGetCode().ToString());
                }
            }

            float[] buffer = new float[sampleRate * 20 * 4]; // 20 seconds buffer
            List<float[]> chunks = new List<float[]>();
            int totalBytesToRead = secondsToRead == 0 ? int.MaxValue : secondsToRead * sampleRate * 4;
            int totalBytesRead = 0;
            while (totalBytesRead < totalBytesToRead)
            {
                // get re-sampled/mono data
                int bytesRead = Bass.BASS_ChannelGetData(mixerStream, buffer, buffer.Length * 4);
                if (bytesRead == 0)
                {
                    break;
                }

                totalBytesRead += bytesRead;

                float[] chunk;

                if (totalBytesRead > totalBytesToRead)
                {
                    chunk = new float[(totalBytesToRead - (totalBytesRead - bytesRead)) / 4];
                    Array.Copy(buffer, chunk, (totalBytesToRead - (totalBytesRead - bytesRead)) / 4);
                }
                else
                {
                    chunk = new float[bytesRead / 4]; // each float contains 4 bytes
                    Array.Copy(buffer, chunk, bytesRead / 4);
                }

                chunks.Add(chunk);
            }

            if (totalBytesRead < (secondsToRead * sampleRate * 4))
            {
                return null; /*not enough samples to return the requested data*/
            }

            float[] data = ConcatenateChunksOfSamples(chunks);

            Bass.BASS_StreamFree(mixerStream);
            Bass.BASS_StreamFree(stream);
            return data;
        }

        public float[] ReadMonoFromURL(string urlToResource, int sampleRate, int secondsToDownload)
        {
            int stream = Bass.BASS_StreamCreateURL(urlToResource, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT, null, IntPtr.Zero);

            if (stream == 0)
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            int mixerStream = BassMix.BASS_Mixer_StreamCreate(sampleRate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            if (mixerStream == 0)
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            if (!BassMix.BASS_Mixer_StreamAddChannel(mixerStream, stream, BASSFlag.BASS_MIXER_FILTER))
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            float[] samples = ReadSamplesFromContinuousMixedStream(sampleRate, secondsToDownload, mixerStream);

            Bass.BASS_StreamFree(mixerStream);
            Bass.BASS_StreamFree(stream);

            return samples;
        }
        
        public float[] RecordFromMicrophone(int sampleRate, int secondsToRecord)
        {
            int stream = Bass.BASS_RecordStart(sampleRate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT, null, IntPtr.Zero);

            if (stream == 0)
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            int mixerStream = BassMix.BASS_Mixer_StreamCreate(sampleRate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            if (mixerStream == 0)
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            if (!BassMix.BASS_Mixer_StreamAddChannel(mixerStream, stream, BASSFlag.BASS_MIXER_FILTER))
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            float[] samples = ReadSamplesFromContinuousMixedStream(sampleRate, secondsToRecord, mixerStream);
            
            Bass.BASS_StreamFree(mixerStream);
            Bass.BASS_StreamFree(stream);

            return samples;
        }

        public float[] RecordFromMicrophoneToFile(string pathToFile, int sampleRate, int secondsToRecord)
        {
            int stream = Bass.BASS_RecordStart(sampleRate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT, null, IntPtr.Zero);

            if (stream == 0)
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            int mixerStream = BassMix.BASS_Mixer_StreamCreate(sampleRate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            if (mixerStream == 0)
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            if (!BassMix.BASS_Mixer_StreamAddChannel(mixerStream, stream, BASSFlag.BASS_MIXER_FILTER))
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }
            
            float[] samples = ReadSamplesFromContinuousMixedStream(sampleRate, secondsToRecord, mixerStream);
            using (WaveWriter waveWriter = new WaveWriter(pathToFile, mixerStream, true))
            {
                waveWriter.Write(samples, samples.Length);
                waveWriter.Close();
            }

            Bass.BASS_StreamFree(mixerStream);
            Bass.BASS_StreamFree(stream);

            return samples;
        }

        public int PlayFile(string filename)
        {
            int stream = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_DEFAULT);
            Bass.BASS_ChannelPlay(stream, false);
            return stream;
        }

        public void StopPlayingFile(int stream)
        {
            if (stream != 0)
            {
                Bass.BASS_StreamFree(stream);
            }
        }

        public void RecodeTheFile(string pathToFile, string outputPathToFile, int targetSampleRate)
        {
            int stream = Bass.BASS_StreamCreateFile(pathToFile, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            TAG_INFO tags = new TAG_INFO();
            BassTags.BASS_TAG_GetFromFile(stream, tags);
            int mixerStream = BassMix.BASS_Mixer_StreamCreate(targetSampleRate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            if (!BassMix.BASS_Mixer_StreamAddChannel(mixerStream, stream, BASSFlag.BASS_MIXER_FILTER))
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            WaveWriter waveWriter = new WaveWriter(outputPathToFile, mixerStream, true);
            const int Length = 5512 * 10 * 4;
            float[] buffer = new float[Length];
            while (true)
            {
                int bytesRead = Bass.BASS_ChannelGetData(mixerStream, buffer, Length);
                if (bytesRead == 0)
                {
                    break;
                }

                waveWriter.Write(buffer, bytesRead);
            }

            waveWriter.Close();
        }

        public TagInfo GetTagInfo(string pathToAudioFile)
        {
            TAG_INFO tags = BassTags.BASS_TAG_GetFromFile(pathToAudioFile);
            TagInfo tag = new TagInfo
                              {
                                  Duration = tags.duration,
                                  Album = tags.album,
                                  Artist = tags.artist,
                                  Title = tags.title,
                                  AlbumArtist = tags.albumartist,
                                  Genre = tags.genre,
                                  Year = tags.year,
                                  Composer = tags.composer
                              };

            return tag;
        }

        public override void Dispose()
        {
            Dispose(false);
            alreadyDisposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!alreadyDisposed)
            {
                if (!isDisposing)
                {
                    // release managed resources
                }

                Bass.BASS_Free();
            }
        }

        private float[] ReadSamplesFromContinuousMixedStream(int sampleRate, int secondsToDownload, int mixerStream)
        {
            float[] buffer = new float[secondsToDownload * sampleRate];
            int totalBytesToRead = secondsToDownload * sampleRate * 4;
            int totalBytesRead = 0;
            List<float[]> chunks = new List<float[]>();
            while (totalBytesRead < totalBytesToRead)
            {
                int bytesRead = Bass.BASS_ChannelGetData(mixerStream, buffer, buffer.Length);
                if (bytesRead == 0)
                {
                    continue;
                }

                totalBytesRead += bytesRead;

                float[] chunk;
                if (totalBytesRead > totalBytesToRead)
                {
                    chunk = new float[(totalBytesToRead - (totalBytesRead - bytesRead)) / 4];
                    Array.Copy(buffer, chunk, (totalBytesToRead - (totalBytesRead - bytesRead)) / 4);
                }
                else
                {
                    chunk = new float[bytesRead / 4];
                    Array.Copy(buffer, chunk, bytesRead / 4);
                }

                chunks.Add(chunk);
            }

            return ConcatenateChunksOfSamples(chunks);
        }
    }
}