namespace SoundFingerprinting.Utils
{
    using System;

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
        public virtual IEncodedFingerprintSchema ExtractTopWavelets(float[] frames, int topWavelets)
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
        public IEncodedFingerprintSchema EncodeFingerprint(float[] concatenated, ushort[] indexes, int topWavelets)
        {
            var schema = new EncodedFingerprintSchema(concatenated.Length * 2);
            for (int i = 0; i < topWavelets; i++)
            {
                int index = indexes[i];
                float value = concatenated[i];
                if (value > 0)
                {
                    schema.SetTrueAt(index * 2);
                }
                else if (value < 0)
                {
                    schema.SetTrueAt((index * 2) + 1);
                }
            }

            return schema;
        }
    }
}