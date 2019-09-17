namespace SoundFingerprinting.Data
{
    using SoundFingerprinting.Utils;

    public class Fingerprint
    {
        public Fingerprint(IEncodedFingerprintSchema schema, float startAt, uint sequenceNumber)
        {
            Schema = schema;
            StartsAt = startAt;
            SequenceNumber = sequenceNumber;
        }

        public IEncodedFingerprintSchema Schema { get; }

        public uint SequenceNumber { get; }

        public float StartsAt { get; }
    }
}
