using System;

namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.LCS;

    public class ExtendedResultEntry : ResultEntry
    {
        public ExtendedResultEntry(TrackData track, Coverage coverage, double confidence, double hammingSimilaritySum, DateTime matchedAt)
            : base(track,
                coverage.SourceMatchStartsAt,
                coverage.SourceMatchLength,
                coverage.SourceCoverageLength,
                coverage.OriginMatchStartsAt,
                coverage.TrackStartsAt,
                confidence, 
                hammingSimilaritySum, 
                coverage.QueryLength, 
                matchedAt)
        {
            ResultCoverage = coverage;
        }

        public Coverage ResultCoverage { get; }
    }
}
