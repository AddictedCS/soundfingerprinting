namespace SoundFingerprinting.Audio
{
    using System;
    using System.IO;

    internal class WaveFormat
    {
        public int SampleRate { get; private set; }

        public short Channels { get; private set; }

        public short BitsPerSample { get; private set; }

        public long Length { get; private set; }

        public float LengthInSeconds
        {
            get
            {
                return (float)Length / (SampleRate * (BitsPerSample / 8) * Channels);
            }
        }

        public static WaveFormat FromFile(string pathToFileName)
        {
            using (var fileStream = new FileStream(pathToFileName, FileMode.Open))
            {
                byte[] header = new byte[44];
                if (fileStream.Read(header, 0, 44) != 44)
                {
                    throw new ArgumentException($"{pathToFileName} is not a valid wav file since it does not contain a header");
                }

                short channels = (short)(header[22] | header[23] << 8);
                int sampleRate = header[24] | header[25] << 8 | header[26] << 16 | header[27] << 24;
                short bitsPerSample = (short)(header[34] | header[35] << 8);
                long bytes = fileStream.Length - 44;

                return new WaveFormat
                       {
                           Channels = channels,
                           SampleRate = sampleRate,
                           BitsPerSample = bitsPerSample,
                           Length = bytes
                       };
            }
        }

        public override string ToString()
        {
            return $"Format: SampleRate ${SampleRate}, Channels ${Channels}, BitsPerSample ${BitsPerSample}, Length ${Length}";
        }
    }
}
