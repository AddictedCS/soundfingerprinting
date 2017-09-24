namespace SoundFingerprinting.Utils
{
    using System;
    using System.Linq;

    /// <summary>
    ///  Signature image encoder/decoder
    /// </summary>
    /// <description>
    ///  Negative Numbers = 01
    ///  Positive Numbers = 10
    ///  Zeros            = 00
    /// </description>
    internal class FingerprintDescriptor : IFingerprintDescriptor
    {
        private readonly AbsComparator absComparator;

        public FingerprintDescriptor()
        {
            absComparator = new AbsComparator();
        }

        /// <summary>
        /// Sets all other wavelet values to 0 except whose which make part of Top Wavelet [top wavelet &gt; 0 ? 1 : -1]
        /// </summary>
        /// <param name="frames">
        /// Frames with 32 logarithmically spaced frequency bins
        /// </param>
        /// <param name="topWavelets">
        /// The top Wavelets.
        /// </param>
        /// <returns>
        /// Signature signature. Array of encoded Boolean elements (wavelet signature)
        /// </returns>
        /// <remarks>
        ///   Negative Numbers = 01
        ///   Positive Numbers = 10
        ///   Zeros            = 00
        /// </remarks>
        public virtual bool[] ExtractTopWavelets(float[] frames, int topWavelets)
        {
            ushort[] indexes = RangeUtils.GetRange(frames.Length); 
            Array.Sort(frames, indexes, absComparator);
            return EncodeFingerprint(frames, indexes, topWavelets);
        }

        /// <summary>
        ///   Encode the integer representation of the fingerprint into a Boolean array
        /// </summary>
        /// <param name = "concatenated">Concatenated fingerprint (frames concatenated)</param>
        /// <param name = "indexes">Sorted indexes with the first one with the highest value in array</param>
        /// <param name = "topWavelets">Number of top wavelets to encode</param>
        /// <returns>Encoded fingerprint</returns>
        public bool[] EncodeFingerprint(float[] concatenated, ushort[] indexes, int topWavelets)
        {
            bool[] result = new bool[concatenated.Length * 2]; // Concatenated float array
            for (int i = 0; i < topWavelets; i++)
            {
                int index = indexes[i];
                double value = concatenated[i];
                if (value > 0)
                {
                    // positive wavelet
                    result[index * 2] = true;
                }
                else if (value < 0)
                {
                    // negative wavelet
                    result[(index * 2) + 1] = true;
                }
            }

            return result;
        }

        /// <summary>
        ///   Decode the signature of the fingerprint
        /// </summary>
        /// <param name = "signature">Signature to be decoded</param>
        /// <returns>Array of doubles with positive [10], negatives [01], and zeros [00]</returns>
        public double[] DecodeFingerprint(bool[] signature)
        {
            int len = signature.Length / 2;
            double[] result = new double[len];
            for (int i = 0; i < len * 2; i += 2)
            {
                if (signature[i])
                {
                    // positive if first is true
                    result[i / 2] = 1;
                }
                else if (signature[i + 1])
                {
                    // negative if second is true
                    result[i / 2] = -1;
                }

                // otherwise '0'
            }

            return result;
        }

        protected float[] ConcatenateFrames(float[][] frames)
        {
            int rows = frames.GetLength(0); /*128*/
            int cols = frames[0].Length; /*32*/
            float[] concatenated = new float[rows * cols]; /* 128 * 32 */
            for (int row = 0; row < rows; row++)
            {
                Buffer.BlockCopy(frames[row], 0, concatenated, row * frames[row].Length * sizeof(float), frames[row].Length * sizeof(float));
            }

            return concatenated;
        }
    }
}