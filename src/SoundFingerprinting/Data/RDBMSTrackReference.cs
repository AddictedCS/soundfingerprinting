namespace SoundFingerprinting.Data
{
    internal class RDBMSTrackReference : ITrackReference
    {
        public RDBMSTrackReference(int id)
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