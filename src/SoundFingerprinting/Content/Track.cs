namespace SoundFingerprinting.Content
{
    using SoundFingerprinting.Data;

    public abstract class Track
    {
        public abstract MediaType Type { get; }

        public abstract double Duration { get; }

        public abstract double TotalEstimatedDuration { get; }
    }
}
