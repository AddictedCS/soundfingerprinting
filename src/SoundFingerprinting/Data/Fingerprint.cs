namespace SoundFingerprinting.Data
{
    using SoundFingerprinting.Utils;

    public class Fingerprint
    {
        public Fingerprint(IEncodedFingerprintSchema schema, float startAt, uint sequenceNumber, byte[] originalPoint)
        {
            Schema = schema;
            StartsAt = startAt;
            SequenceNumber = sequenceNumber;
            OriginalPoint = originalPoint;
        }

        public IEncodedFingerprintSchema Schema { get; }

        public uint SequenceNumber { get; }

        public float StartsAt { get; }

        public byte[] OriginalPoint { get;  }
    }
}
