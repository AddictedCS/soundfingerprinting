namespace SoundFingerprinting.Configuration.Frames
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;
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
            QueryPathReconstructionStrategy = QueryPathReconstructionStrategy.Legacy;
            YesMetaFieldsFilters = new Dictionary<string, string>();
            NoMetaFieldsFilters = new Dictionary<string, string>();

            FingerprintConfiguration = new DefaultVideoFingerprintConfiguration
            {
                // lets get a copy of original point even if we may not actually need it
                // since we will allow per track SSIM filtering, we need to grab a copy anyways
                OriginalPointSaveTransform = frame => frame.GetQuantizedCopy()
            };

            ThresholdVotes = 4;
            PermittedGap = 1.75d;
            OutliersFilterConfiguration = OutliersFilterConfiguration.None;
            StructuralSimilarityFilterConfiguration = StructuralSimilarityFilterConfiguration.None;
            QueryMediaType = MediaType.Video;
        }
    }
}
