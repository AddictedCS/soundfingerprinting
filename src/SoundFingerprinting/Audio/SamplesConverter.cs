namespace SoundFingerprinting.Audio
{
    using System;

    public class SamplesConverter
    {
        public static float[] GetFloatSamplesFromByte(int bytesRecorded, byte[] buffer)
        {
            int startIndex = 0;
            float[] chunk = new float[bytesRecorded / 4];
            int floatSampleIndex = 0;
            while (startIndex < bytesRecorded)
            {
                chunk[floatSampleIndex++] = BitConverter.ToSingle(buffer, startIndex);
                startIndex += 4;
            }

            return chunk;
        }
    }
}
