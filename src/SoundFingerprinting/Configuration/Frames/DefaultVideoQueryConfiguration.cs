namespace SoundFingerprinting.Configuration.Frames
{
    using System.Collections.Generic;
    using SoundFingerprinting.LCS;

    /// <summary>
    ///  Video query configuration class.
    /// </summary>
    public class DefaultVideoQueryConfiguration : VideoQueryConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultVideoQueryConfiguration"/> class.
        /// </summary>
        public DefaultVideoQueryConfiguration()
        {
            MaxTracksToReturn = 25;
            QueryPathReconstructionStrategy = QueryPathReconstructionStrategyType.Legacy;
            YesMetaFieldsFilters = new Dictionary<string, string>();
            NoMetaFieldsFilters = new Dictionary<string, string>();

            FingerprintConfiguration = new DefaultVideoFingerprintConfiguration
            {
                // let's get a copy of original point even if we may not actually need it
                // since we will allow per track SSIM filtering, we need to grab a copy anyway
                OriginalPointSaveTransform = frame => frame.GetQuantizedCopy()
            };

            TruePositivesFilter = new AllMatchesAreTruePositives();
            ThresholdVotes = 4;
            PermittedGap = 1.75d;
            StructuralSimilarityFilterConfiguration = StructuralSimilarityFilterConfiguration.None;
        }
    }
}
