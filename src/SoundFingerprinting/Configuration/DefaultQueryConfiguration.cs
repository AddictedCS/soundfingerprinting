namespace SoundFingerprinting.Configuration
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;

    public class DefaultQueryConfiguration : QueryConfiguration
    {
        public DefaultQueryConfiguration()
        {
            ThresholdVotes = Configs.Threshold.Default;
            MaxTracksToReturn = 25;
            PermittedGap = 2d;
            AllowMultipleMatchesOfTheSameTrackInQuery = false;
            FingerprintConfiguration = new DefaultFingerprintConfiguration
                                       {
                                           Stride = Configs.QueryStrides.DefaultStride,
                                           FrequencyRange = Configs.FrequencyRanges.Default
                                       };
            MetaFieldsFilter = new Dictionary<string, string>();
            QueryMediaType = MediaType.Audio;
        }
    }
}
