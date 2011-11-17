// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.Misc;

namespace Soundfingerprinting.AudioProxies
{
    /// <summary>
    ///   Bass Proxy for Bass.Net API
    /// </summary>
    /// <remarks>
    ///   BASS is an audio library for use in Windows and Mac OSX software. 
    ///   Its purpose is to provide developers with powerful and efficient sample, stream (MP3, MP2, MP1, OGG, WAV, AIFF, custom generated, and more via add-ons), 
    ///   MOD music (XM, IT, S3M, MOD, MTM, UMX), MO3 music (MP3/OGG compressed MODs), and recording functions. 
    ///   All in a tiny DLL, under 100KB* in size.
    /// </remarks>
    public class BassProxy : IAudio
    {
        /// <summary>
        ///   Default sample rate used at initialization
        /// </summary>
        private const int DEFAULT_SAMPLE_RATE = 44100;

        /// <summary>
        ///   Reference counter used in order to determine when to unload the native C library
        /// </summary>
        private static volatile int _referenceCounter;

        /// <summary>
        ///   Checks whether the library is initialized
        /// </summary>
        private static volatile bool _initialized;

        /// <summary>
        ///   Global lock object
        /// </summary>
        private static readonly object Lockobject = new object();

        /// <summary>
        ///   Shows whether the proxy is already disposed
        /// </summary>
        private bool _alreadyDisposed;

        /// <summary>
        ///   Currently playing stream
        /// </summary>
        private int _playingStream;

        #region Constructors

        /// <summary>
        ///   Public Constructor
        /// </summary>
        public BassProxy()
        {
            lock (Lockobject)
            {
                if (_referenceCounter == 0) //first instance in the project, register and load the assemblies
                {
                    if (!_initialized)
                    {
                        //Call to avoid the freeware splash screen. Didn't see it, but maybe it will appear if the Forms are used :D
                        BassNet.Registration("gleb.godonoga@gmail.com", "2X155323152222");

                        //Dummy calls made for loading the assemblies
                        int bassVersion = Bass.BASS_GetVersion();
                        int bassMixVersion = BassMix.BASS_Mixer_GetVersion();
                        int bassfxVersion = BassFx.BASS_FX_GetVersion();

                        int plg = Bass.BASS_PluginLoad("bassflac.dll");
                        if (plg == 0)
                            throw new Exception(Bass.BASS_ErrorGetCode().ToString());
                        if (!Bass.BASS_Init(-1, DEFAULT_SAMPLE_RATE, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_MONO, IntPtr.Zero)) //Set Sample Rate / MONO
                            throw new Exception(Bass.BASS_ErrorGetCode().ToString());
                        if (!Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_MIXER_FILTER, 50)) /*Set filter for anti aliasing*/
                            throw new Exception(Bass.BASS_ErrorGetCode().ToString());
                        if (!Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_FLOATDSP, true)) /*Set floating parameters to be passed*/
                            throw new Exception(Bass.BASS_ErrorGetCode().ToString());

                        _initialized = true;
                    }
                }
                _referenceCounter++;
            }
        }

        #endregion

        #region IAudio Members

        /// <summary>
        ///   Dispose the unmanaged resource. Free bass.dll.
        /// </summary>
        public void Dispose()
        {
            Dispose(false);
            _alreadyDisposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Read mono from file
        /// </summary>
        /// <param name = "filename">Name of the file</param>
        /// <param name = "samplerate">Output sample rate</param>
        /// <param name = "milliseconds">Milliseconds to read</param>
        /// <param name = "startmillisecond">Start millisecond</param>
        /// <returns>Array of samples</returns>
        /// <remarks>
        ///   Seeking capabilities of Bass where not used because of the possible
        ///   timing errors on different formats.
        /// </remarks>
        public float[] ReadMonoFromFile(string filename, int samplerate, int milliseconds, int startmillisecond)
        {
            int totalmilliseconds = milliseconds <= 0 ? Int32.MaxValue : milliseconds + startmillisecond;
            float[] data = null;
            //create streams for re-sampling
            int stream = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT); //Decode the stream
            if (stream == 0)
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            int mixerStream = BassMix.BASS_Mixer_StreamCreate(samplerate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            if (mixerStream == 0)
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());

            if (BassMix.BASS_Mixer_StreamAddChannel(mixerStream, stream, BASSFlag.BASS_MIXER_FILTER))
            {
                int bufferSize = samplerate*20*4; /*read 10 seconds at each iteration*/
                float[] buffer = new float[bufferSize];
                List<float[]> chunks = new List<float[]>();
                int size = 0;
                while ((float) (size)/samplerate*1000 < totalmilliseconds)
                {
                    //get re-sampled/mono data
                    int bytesRead = Bass.BASS_ChannelGetData(mixerStream, buffer, bufferSize);
                    if (bytesRead == 0)
                        break;
                    float[] chunk = new float[bytesRead/4]; //each float contains 4 bytes
                    Array.Copy(buffer, chunk, bytesRead/4);
                    chunks.Add(chunk);
                    size += bytesRead/4; //size of the data
                }

                if ((float) (size)/samplerate*1000 < (milliseconds + startmillisecond))
                    return null; /*not enough samples to return the requested data*/
                int start = (int) ((float) startmillisecond*samplerate/1000);
                int end = (milliseconds <= 0) ? size : (int) ((float) (startmillisecond + milliseconds)*samplerate/1000);
                data = new float[size];
                int index = 0;
                /*Concatenate*/
                foreach (float[] chunk in chunks)
                {
                    Array.Copy(chunk, 0, data, index, chunk.Length);
                    index += chunk.Length;
                }
                /*Select specific part of the song*/
                if (start != 0 || end != size)
                {
                    float[] temp = new float[end - start];
                    Array.Copy(data, start, temp, 0, end - start);
                    data = temp;
                }
            }
            else
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            return data;
        }

        #endregion

        /// <summary>
        ///   Read data from file
        /// </summary>
        /// <param name = "filename">Filename to be read</param>
        /// <param name = "samplerate">Sample rate at which to perform reading</param>
        /// <returns>Array with data</returns>
        public float[] ReadMonoFromFile(string filename, int samplerate)
        {
            return ReadMonoFromFile(filename, samplerate, 0, 0);
        }

        public float[][] ReadSpectrum(string filename, int samplerate, int startmillisecond, int milliseconds, int overlap, int wdftsize, int logbins, int startfreq, int endfreq)
        {
            int totalmilliseconds = 0;
            if (milliseconds <= 0)
                totalmilliseconds = Int32.MaxValue;
            else
                totalmilliseconds = milliseconds + startmillisecond;
            const int logbase = 2;
            double logMin = Math.Log(startfreq, logbase);
            double logMax = Math.Log(endfreq, logbase);
            double delta = (logMax - logMin)/logbins;
            double accDelta = 0;
            float[] freqs = new float[logbins + 1];
            for (int i = 0; i <= logbins /*32 octaves*/; ++i)
            {
                freqs[i] = (float) Math.Pow(logbase, logMin + accDelta);
                accDelta += delta; // accDelta = delta * i
            }

            List<float[]> data = new List<float[]>();
            int[] streams = new int[wdftsize/overlap - 1];
            int[] mixerstreams = new int[wdftsize/overlap - 1];
            double sec = (double) overlap/samplerate;
            for (int i = 0; i < wdftsize/overlap - 1; i++)
            {
                streams[i] = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT); //Decode the stream
                if (!Bass.BASS_ChannelSetPosition(streams[i], (float) startmillisecond/1000 + sec*i))
                    throw new Exception(Bass.BASS_ErrorGetCode().ToString());
                mixerstreams[i] = BassMix.BASS_Mixer_StreamCreate(samplerate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
                if (!BassMix.BASS_Mixer_StreamAddChannel(mixerstreams[i], streams[i], BASSFlag.BASS_MIXER_FILTER))
                    throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }

            float[] buffer = new float[wdftsize/2];
            int size = 0;
            int iter = 0;
            while ((float) (size)/samplerate*1000 < totalmilliseconds)
            {
                int bytesRead = Bass.BASS_ChannelGetData(mixerstreams[iter%(wdftsize/overlap - 1)], buffer, (int) BASSData.BASS_DATA_FFT2048);
                if (bytesRead == 0)
                    break;
                float[] chunk = new float[logbins];
                for (int i = 0; i < logbins; i++)
                {
                    int lowBound = (int) freqs[i];
                    int endBound = (int) freqs[i + 1];
                    int startIndex = Utils.FFTFrequency2Index(lowBound, wdftsize, samplerate);
                    int endIndex = Utils.FFTFrequency2Index(endBound, wdftsize, samplerate);
                    float sum = 0f;
                    for (int j = startIndex; j < endIndex; j++)
                    {
                        sum += buffer[j];
                    }
                    chunk[i] = sum/(endIndex - startIndex);
                }
                data.Add(chunk);
                size += bytesRead/4;
                iter++;
            }


            return data.ToArray();
        }

        /// <summary>
        ///   Get's tag info from file
        /// </summary>
        /// <param name = "filename">Filename to decode</param>
        /// <returns>TAG_INFO structure</returns>
        /// <remarks>
        ///   The tags can be extracted using the following code:
        ///   <code>
        ///     tags.album
        ///     tags.albumartist
        ///     tags.artist
        ///     tags.title
        ///     tags.duration
        ///     tags.genre, and so on.
        ///   </code>
        /// </remarks>
        public TAG_INFO GetTagInfoFromFile(string filename)
        {
            return BassTags.BASS_TAG_GetFromFile(filename);
        }

        /// <summary>
        ///   Play file
        /// </summary>
        /// <param name = "filename">Filename</param>
        public void PlayFile(string filename)
        {
            int stream = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_DEFAULT);
            Bass.BASS_ChannelPlay(stream, false);
            _playingStream = stream;
        }

        public void StopPlayingFile()
        {
            if (_playingStream != 0)
                Bass.BASS_StreamFree(_playingStream);
        }


        /// <summary>
        ///   Recode the file
        /// </summary>
        /// <param name = "fileName">Initial file</param>
        /// <param name = "outFileName">Target file</param>
        /// <param name = "targetSampleRate">Target sample rate</param>
        public void RecodeTheFile(string fileName, string outFileName, int targetSampleRate)
        {
            int stream = Bass.BASS_StreamCreateFile(fileName, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            TAG_INFO tags = new TAG_INFO();
            BassTags.BASS_TAG_GetFromFile(stream, tags);
            int mixerStream = BassMix.BASS_Mixer_StreamCreate(targetSampleRate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
            if (BassMix.BASS_Mixer_StreamAddChannel(mixerStream, stream, BASSFlag.BASS_MIXER_FILTER))
            {
                WaveWriter waveWriter = new WaveWriter(outFileName, mixerStream, true);
                const int length = 5512*10*4;
                float[] buffer = new float[length];
                while (true)
                {
                    int bytesRead = Bass.BASS_ChannelGetData(mixerStream, buffer, length);
                    if (bytesRead == 0)
                        break;
                    waveWriter.Write(buffer, bytesRead);
                }
                waveWriter.Close();
            }
            else
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
        }

        /// <summary>
        ///   Dispose the resources
        /// </summary>
        /// <param name = "isDisposing">If value is disposing</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (!_alreadyDisposed)
            {
                if (!isDisposing)
                {
                    //release managed resources
                }

                //release unmanaged resources
                lock (Lockobject)
                {
                    _referenceCounter--;
                    //if (_referenceCounter == 0) //last instance in the project, release BASS
                    //    Bass.BASS_Free();
                }
            }
        }

        /// <summary>
        ///   Finalizer
        /// </summary>
        ~BassProxy()
        {
            Dispose(true);
        }
    }
}