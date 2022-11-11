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
            QueryPathReconstructionStrategy queryPathReconstructionStrategy,
            double queryLength, 
            double trackLength, 
            double fingerprintLength, 
            double permittedGap)
        {
            var reconstructedPaths = queryPathReconstructionStrategy switch
            {
                QueryPathReconstructionStrategy.Legacy => new LegacyQueryPathReconstructionStrategy(fingerprintLength).GetBestPaths(matchedEntries, Math.Min(queryLength, trackLength)),
                QueryPathReconstructionStrategy.SingleBestPath => Single.GetBestPaths(matchedEntries, -1),
                QueryPathReconstructionStrategy.MultipleBestPaths => Multiple.GetBestPaths(matchedEntries, permittedGap),
                _ => throw new NotSupportedException($"Provided path reconstruction strategy is not valid {queryPathReconstructionStrategy}")
            };
            
            var coverages = reconstructedPaths.Select(sequence => new Coverage(sequence, queryLength, trackLength, fingerprintLength, permittedGap));
            if (queryPathReconstructionStrategy == QueryPathReconstructionStrategy.MultipleBestPaths)
            {
                return OverlappingRegionFilter.FilterCrossMatchedCoverages(coverages); 
            }

            return coverages.ToList();
        }
    }
}
