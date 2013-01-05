// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.DirectX.DirectSound;
using Buffer = Microsoft.DirectX.DirectSound.Buffer;

namespace Soundfingerprinting.AudioProxies
{
    /// <summary>
    ///   Direct Sound Wrapper.
    /// </summary>
    /// <remarks>
    ///   This class reads can perform asynchronous read from *.wav files. No other types of files are supported, as Microsoft Direct Sound technology does not allow other formats. 
    ///   The file format (Bit rate, Sampling rate, etc.) is kept as it is, such that no conversion task is performed.
    ///   Before using this class please uncheck the Loader Lock exception from Debug/Exceptions menu, in order for the CLR be able to Load DirectSound COM components. 
    ///   x86 architecture is the only supported.
    /// </remarks>
    [Obsolete("Use BassAudioService instead")]
    public class DirectSoundProxy : IAudioService
    {
        #region Private Variables

        private bool _alreadyDisposed; /*Disposed state param*/

        #endregion

        #region IAudioService Members

        /// <summary>
        ///   Dispose the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            _alreadyDisposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Read an audio file using mono format
        /// </summary>
        /// <param name = "fileName">File to read from. Should be a .wav file</param>
        /// <param name = "sampleRate">
        ///   Sample rate of the file. This proxy does not support down or up sampling.
        ///   Please convert the file to appropriate sample, and then use this method.
        /// </param>
        /// <param name = "milliSeconds">Milliseconds to read</param>
        /// <param name = "startMilliSeconds">Start millisecond</param>
        /// <returns>Audio samples</returns>
        public float[] ReadMonoFromFile(string fileName, int sampleRate, int milliSeconds, int startMilliSeconds)
        {
            int totalmilliseconds = milliSeconds <= 0 ? Int32.MaxValue : milliSeconds + startMilliSeconds;
            if (_alreadyDisposed)
                throw new ObjectDisposedException("Object already disposed");
            if (Path.GetExtension(fileName) != ".wav")
                throw new ArgumentException("DirectSound can read only .wav files. Please transform your input file into appropriate type.");
            Device device = new Device(new DevicesCollection()[0].DriverGuid);
            Buffer buffer = new Buffer(Path.GetFullPath(fileName), device);

            /*Default sound card is used as parent Device*/
            long fileSize = buffer.Caps.BufferBytes;
            int offset = 0;
            int bytesPerSample = buffer.Format.BitsPerSample/8;
            const int dwOutputBufferSize = 5512*10*4;
            List<float[]> chunks = new List<float[]>();
            int size = 0;
            try
            {
                while ((float) (size)/sampleRate*1000 < totalmilliseconds)
                {
                    byte[] ar = (byte[]) buffer.Read(offset, typeof (byte), LockFlag.EntireBuffer, dwOutputBufferSize);
                    offset += dwOutputBufferSize;
                    long readData = offset > (fileSize) ? fileSize - (offset - dwOutputBufferSize) : ar.Length;
                    float[] result = new float[readData/bytesPerSample];
                    for (int i = 0; i < result.Length; i++)
                    {
                        switch (bytesPerSample)
                        {
                            case 2:
                                result[i] = BitConverter.ToInt16(ar, i*bytesPerSample);
                                break;
                            case 4:
                                result[i] = BitConverter.ToInt32(ar, i*bytesPerSample);
                                break;
                        }
                    }
                    chunks.Add(result);
                    size += result.Length;
                    if (offset > fileSize)
                        break;
                }
            }
            finally
            {
                buffer.Stop();
                buffer.Dispose();
                device.Dispose();
            }

            if ((float) (size)/sampleRate*1000 < (milliSeconds + startMilliSeconds))
                return null; /*not enough samples to return the requested data*/
            int start = (int) ((float) startMilliSeconds*sampleRate/1000);
            int end = (milliSeconds <= 0) ? size : (int) ((float) (startMilliSeconds + milliSeconds)*sampleRate/1000);
            float[] data = new float[size];
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
            return data;
        }

        #endregion

        public float[] ReadMonoFromFile(string fileName, int sampleRate)
        {
            return ReadMonoFromFile(fileName, sampleRate, 0, 0);
        }

        /// <summary>
        ///   Dispose unmanaged resources
        /// </summary>
        /// <param name = "isDisposing">If is disposing</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (!_alreadyDisposed)
            {
                if (isDisposing)
                {
                    /*release managed resources*/
                }
            }
        }

        /// <summary>
        ///   Finalizer
        /// </summary>
        ~DirectSoundProxy()
        {
            Dispose(false);
        }
    }
}