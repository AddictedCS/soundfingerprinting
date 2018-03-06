namespace SoundFingerprinting.Audio
{
    using System.IO;

    internal class WaveFormat
    {
        public int SampleRate { get; private set; }

        public short Channels { get; private set; }

        public short BitsPerSample { get; private set; }

        public int Length { get; private set; }

        public float LengthInSeconds
        {
            get
            {
                return (float)Length / (SampleRate * (BitsPerSample / 8) * Channels);
            }
        }

        public static WaveFormat FromFile(string pathToFileName)
        {
            using (var binaryReader = new BinaryReader(new FileStream(pathToFileName, FileMode.Open)))
            {
                byte[] header = binaryReader.ReadBytes(44);

                short channels = (short)(header[22] | header[23] << 8);
                int sampleRate = header[24] | header[25] << 8 | header[26] << 16 | header[27] << 24;
                short bitsPerSample = (short)(header[34] | header[35] << 8);
                int bytes = header[40] | header[41] << 8 | header[42] << 16 | header[43] << 24;

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
