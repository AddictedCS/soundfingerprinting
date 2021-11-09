namespace SoundFingerprinting.Configuration.Frames
{
    using SoundFingerprinting.Configuration;

    public abstract class VideoQueryConfiguration : DefaultQueryConfiguration
    {
        public OutliersFilterConfiguration OutliersFilterConfiguration { get; set; } = null!;

        public StructuralSimilarityFilterConfiguration StructuralSimilarityFilterConfiguration { get; set; } = null!;
    }
}
