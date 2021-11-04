namespace SoundFingerprinting.Configuration.Frames
{
    using SoundFingerprinting.Data;

    public class DefaultVideoQueryConfiguration : VideoQueryConfiguration
    {
        public DefaultVideoQueryConfiguration()
        {
            FingerprintConfiguration = new DefaultVideoFingerprintConfiguration
            {
                // lets get a copy of original point even if we may not actually need it
                // since we will allow per track SSIM filtering, we need to grab a copy anyways
                OriginalPointSaveTransform = frame => frame.GetQuantizedCopy()
            };
            
            ThresholdVotes = 4;
            PermittedGap = 1.75d;
            QueryMediaType = MediaType.Video;
            OutliersFilterConfiguration = OutliersFilterConfiguration.None;
            StructuralSimilarityFilterConfiguration = StructuralSimilarityFilterConfiguration.None;
        }
    }
}
