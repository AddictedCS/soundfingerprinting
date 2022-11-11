namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SoundFingerprinting.LCS;

    public static class MatchedWithExtensions
    {
        private static readonly IQueryPathReconstructionStrategy Single = new SingleQueryPathReconstructionStrategy();
        private static readonly IQueryPathReconstructionStrategy Multiple = new MultipleQueryPathReconstructionStrategy();
        
        public static IEnumerable<Coverage> GetCoverages(
            this IEnumerable<MatchedWith> matchedEntries, 
            QueryPathReconstructionStrategyType queryPathReconstructionStrategyType,
            double queryLength, 
            double trackLength, 
            double fingerprintLength, 
            double permittedGap)
        {
            var reconstructedPaths = queryPathReconstructionStrategyType switch
            {
                QueryPathReconstructionStrategyType.Legacy => new LegacyQueryPathReconstructionStrategy(fingerprintLength).GetBestPaths(matchedEntries, Math.Min(queryLength, trackLength)),
                QueryPathReconstructionStrategyType.SingleBestPath => Single.GetBestPaths(matchedEntries, -1),
                QueryPathReconstructionStrategyType.MultipleBestPaths => Multiple.GetBestPaths(matchedEntries, permittedGap),
                _ => throw new NotSupportedException($"Provided path reconstruction strategy is not valid {queryPathReconstructionStrategyType}")
            };
            
            var coverages = reconstructedPaths.Select(sequence => new Coverage(sequence, queryLength, trackLength, fingerprintLength, permittedGap));
            if (queryPathReconstructionStrategyType == QueryPathReconstructionStrategyType.MultipleBestPaths)
            {
                return OverlappingRegionFilter.FilterCrossMatchedCoverages(coverages); 
            }

            return coverages.ToList();
        }
    }
}
