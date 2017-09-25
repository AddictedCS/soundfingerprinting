namespace SoundFingerprinting.Data
{
    using SoundFingerprinting.Utils;

    internal class Fingerprint
    {
        public Fingerprint(IEncodedFingerprintSchema signature, double startAt, int sequenceNumber)
        {
            Signature = signature;
            StartsAt = startAt;
            SequenceNumber = sequenceNumber;
        }

        public IEncodedFingerprintSchema Signature { get; private set; }

        public int SequenceNumber { get; private set; }

        public double StartsAt { get; private set; }
    }
}
