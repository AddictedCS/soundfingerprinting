namespace SoundFingerprinting.Data
{
    internal class RDBMSSubFingerprintReference : ISubFingerprintReference
    {
        public RDBMSSubFingerprintReference(long id)
        {
            Id = id;
        }

        public long Id { get; private set; }

        public int HashCode
        {
            get
            {
                return Id.GetHashCode();
            }
        }
    }
}
