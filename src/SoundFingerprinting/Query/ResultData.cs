namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Dao.Entities;

    public class ResultData
    {
        public Track Track { get; set; }

        public int Similarity { get; set; }
    }
}