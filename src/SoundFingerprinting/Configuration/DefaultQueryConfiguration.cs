﻿namespace SoundFingerprinting.Configuration
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.LCS;

    /// <inheritdoc cref="QueryConfiguration"/>
    public class DefaultQueryConfiguration : QueryConfiguration
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="DefaultQueryConfiguration"/> class.
        /// </summary>
        public DefaultQueryConfiguration()
        {
            ThresholdVotes = Configs.Threshold.Default;
            MaxTracksToReturn = 25;
            PermittedGap = 2d;
            QueryPathReconstructionStrategy = QueryPathReconstructionStrategyType.MultipleBestPaths;
            FingerprintConfiguration = new DefaultFingerprintConfiguration
                                       {
                                           Stride = Configs.QueryStrides.DefaultStride,
                                           FrequencyRange = Configs.FrequencyRanges.Default
                                       };
            YesMetaFieldsFilters = new Dictionary<string, string>();
            NoMetaFieldsFilters = new Dictionary<string, string>();
            QueryMediaType = MediaType.Audio;
        }
    }
}
