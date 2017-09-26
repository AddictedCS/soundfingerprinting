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

        public IEncodedFingerprintSchema Signature { get; private set; }

        public uint SequenceNumber { get; private set; }

        public float StartsAt { get; private set; }
    }
}
