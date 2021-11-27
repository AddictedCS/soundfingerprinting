namespace SoundFingerprinting.Data
{
    using SoundFingerprinting.Utils;

    /// <summary>
    ///  Class that represents image features encoded with a fingerprinting schema defined by <see cref="IEncodedFingerprintSchema"/>.
    /// </summary>
    /// <remarks>
    ///  You can read more about fingerprints which are not yet hashed via min-hashing <a href="https://emysound.com/blog/open-source/2020/06/12/how-audio-fingerprinting-works.html">here</a>.
    ///  This class hold an intermediate representation of the fingerprint which is too big to be stored, and thus is min-hashes to further reduce the dimensionality of the data.
    /// </remarks>
    public class Fingerprint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Fingerprint"/> class.
        /// </summary>
        /// <param name="schema">Encoded fingerprint schema.</param>
        /// <param name="startsAt">Starts at reference point (measured in seconds).</param>
        /// <param name="sequenceNumber">Sequence number.</param>
        /// <param name="originalPoint">Original point (if any).</param>
        public Fingerprint(IEncodedFingerprintSchema schema, float startsAt, uint sequenceNumber, byte[] originalPoint)
        {
            Schema = schema;
            StartsAt = startsAt;
            SequenceNumber = sequenceNumber;
            OriginalPoint = originalPoint;
        }

        /// <summary>
        ///  Gets encoded fingerprint schema.
        /// </summary>
        public IEncodedFingerprintSchema Schema { get; }

        /// <summary>
        ///  Gets sequence number.
        /// </summary>
        public uint SequenceNumber { get; }

        /// <summary>
        ///  Gets starts at time.
        /// </summary>
        public float StartsAt { get; }

        /// <summary>
        ///  Gets original point if any.
        /// </summary>
        public byte[] OriginalPoint { get; }
    }
}