namespace SoundFingerprinting.Configuration
{
    using System.Collections.Generic;
    using System.Linq;

    public class DefaultQueryConfiguration : QueryConfiguration
    {
        public DefaultQueryConfiguration()
        {
            ThresholdVotes = Configs.Threshold.Default;
            MaxTracksToReturn = 25;
            Clusters = Enumerable.Empty<string>();
            PermittedGap = 2d;
            AllowMultipleMatchesOfTheSameTrackInQuery = false;
            FingerprintConfiguration = new DefaultFingerprintConfiguration
                                       {
                                           Stride = Configs.QueryStrides.DefaultStride,
                                           FrequencyRange = Configs.FrequencyRanges.Default
                                       };
            MetaFields = new Dictionary<string, string>();
        }
    }
}
