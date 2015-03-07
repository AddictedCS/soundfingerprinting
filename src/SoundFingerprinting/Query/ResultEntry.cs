namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public class ResultEntry
    {
        public TrackData Track { get; set; }

        public int Similarity { get; set; }
    }
}