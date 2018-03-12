namespace SoundFingerprinting.Data
{
    using SoundFingerprinting.Utils;

    internal class Fingerprint
    {
        public Fingerprint(IEncodedFingerprintSchema signature, float startAt, uint sequenceNumber)
        {
            Signature = signature;
            StartsAt = startAt;
            SequenceNumber = sequenceNumber;
        }

        public IEncodedFingerprintSchema Signature { get; }

        public uint SequenceNumber { get; }

        public float StartsAt { get; }
    }
}
