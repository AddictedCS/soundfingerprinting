namespace Soundfingerprinting.Audio.NAudio
{
    using System;
    using System.Collections.Generic;

    using global::NAudio.Wave;
    using global::NAudio.Wave.SampleProviders;

    public class NAudioService : AudioService, IExtendedAudioService
    {
        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override float[] ReadMonoFromFile(string pathToFile, int sampleRate, int secondsToRead, int startAtSecond)
        {
            using (var reader = new MediaFoundationReader(pathToFile))
            {
                int actualSampleRate = reader.WaveFormat.SampleRate;
                int bitsPerSample = reader.WaveFormat.BitsPerSample;
                reader.Seek(actualSampleRate * bitsPerSample / 8 * startAtSecond / 1000, System.IO.SeekOrigin.Begin);
                using (var resampler = new MediaFoundationResampler(reader, WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1)))
                {
                    float[] buffer = new float[sampleRate]; // 20 seconds buffer
                    List<float[]> chunks = new List<float[]>();
                    int totalFloatsToRead = secondsToRead == 0 ? int.MaxValue : secondsToRead / 1000 * sampleRate / 32;
                    int totalFloatsRead = 0;
                    Pcm32BitToSampleProvider pcmReader = new Pcm32BitToSampleProvider(resampler);
                    while (totalFloatsRead < totalFloatsToRead)
                    {
                        // get re-sampled/mono data
                        int floatsRead = pcmReader.Read(buffer, 0, buffer.Length);
                        if (floatsRead == 0)
                        {
                            break;
                        }

                        totalFloatsRead += floatsRead;

                        float[] chunk;

                        if (totalFloatsRead > totalFloatsToRead)
                        {
                            chunk = new float[(totalFloatsToRead - (totalFloatsRead - floatsRead))];
                            Array.Copy(buffer, chunk, totalFloatsToRead - (totalFloatsRead - floatsRead));
                        }
                        else
                        {
                            chunk = new float[floatsRead]; // each float contains 4 bytes
                            Array.Copy(buffer, chunk, floatsRead);
                        }

                        chunks.Add(chunk);
                    }

                    if (totalFloatsRead < (secondsToRead / 1000 * sampleRate))
                    {
                        return null; /*not enough samples to return the requested data*/
                    }

                    float[] data = ConcatenateChunksOfSamples(chunks);

                    return data;
                }
            }
        }

        public void SetAllignedFloatArrayFromByte(byte[] array, float[] target, int length)
        {
            for (int i = 0, n = length; i < n; i++)
            {
                target[i] = BitConverter.ToSingle(array, i * 4);
            }
        }

        public bool IsRecordingSupported
        {
            get
            {
                return false;
            }
        }

        public int PlayFile(string filename)
        {
            throw new NotImplementedException();
        }

        public void StopPlayingFile(int stream)
        {
            throw new NotImplementedException();
        }

        public void RecodeTheFile(string pathToFile, string outputPathToFile, int targetSampleRate)
        {
            throw new NotImplementedException();
        }

        public float[] ReadMonoFromURL(string urlToResource, int sampleRate, int secondsToDownload)
        {
            throw new NotImplementedException();
        }

        public float[] RecordFromMicrophoneToFile(string pathToFile, int sampleRate, int secondsToRecord)
        {
            throw new NotImplementedException();
        }
    }
}
