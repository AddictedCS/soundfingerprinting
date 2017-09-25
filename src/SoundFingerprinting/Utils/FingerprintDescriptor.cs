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

        public virtual IEncodedFingerprintSchema ExtractTopWavelets(float[] frames, int topWavelets, ushort[] indexes)
        {
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