namespace SoundFingerprinting.Data
{
    internal class RDBMSFingerprintReference : IFingerprintReference
    {
        public RDBMSFingerprintReference(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }

        public int HashCode
        {
            get
            {
                return Id;
            }
        }
    }
}
