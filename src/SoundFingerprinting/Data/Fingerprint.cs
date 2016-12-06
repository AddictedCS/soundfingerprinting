namespace SoundFingerprinting.Data
{
    internal class Fingerprint
    {
        public Fingerprint(bool[] signature, double startAt, int sequenceNumber)
        {
            Signature = signature;
            StartsAt = startAt;
            SequenceNumber = sequenceNumber;
        }

        public bool[] Signature { get; private set; }

        public int SequenceNumber { get; private set; }

        public double StartsAt { get; private set; }
    }
}
