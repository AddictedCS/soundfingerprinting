namespace SoundFingerprinting.Utils
{
    /// <summary>
    ///  Signature image encoder/decoder
    /// </summary>
    /// <description>
    ///  Negative Numbers = 01
    ///  Positive Numbers = 10
    ///  Zeros            = 00
    /// </description>
    internal class FingerprintEncoder : IFingerprintEncoder
    {
        /// <summary>
        ///   Encode the integer representation of the fingerprint into a Boolean array
        /// </summary>
        /// <param name = "concatenated">Concatenated fingerprint (frames concatenated)</param>
        /// <param name = "indexes">Sorted indexes with the first one with the highest value in array</param>
        /// <param name = "topWavelets">Number of top wavelets to encode</param>
        /// <returns>Encoded fingerprint</returns>
        public IEncodedFingerprintSchema EncodeFingerprint(float[] concatenated, ushort[] indexes, int topWavelets)
        {
            TinyFingerprintSchema schema = new TinyFingerprintSchema(concatenated.Length * 2);
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
