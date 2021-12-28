namespace SoundFingerprinting.Configuration.Frames
{
    using SoundFingerprinting.Configuration;

    /// <summary>
    ///  Video query configuration class.
    /// </summary>
    public abstract class VideoQueryConfiguration : QueryConfiguration
    {
        /// <summary>
        ///  Gets or sets outliers filter configuration.
        /// </summary>
        public OutliersFilterConfiguration OutliersFilterConfiguration { get; set; } = null!;

        /// <summary>
        ///  Gets or sets image structural similarity filter.
        /// </summary>
        public StructuralSimilarityFilterConfiguration StructuralSimilarityFilterConfiguration { get; set; } = null!;
    }
}
