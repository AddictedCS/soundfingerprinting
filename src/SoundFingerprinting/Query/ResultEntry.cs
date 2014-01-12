namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Data;

    public class ResultEntry
    {
        public TrackData Track { get; set; }

        public int Similarity { get; set; }
    }
}