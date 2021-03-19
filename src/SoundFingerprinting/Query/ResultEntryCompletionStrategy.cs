namespace SoundFingerprinting.Query
{
    using System;

    public sealed class ResultEntryCompletionStrategy : ICompletionStrategy<ResultEntry>
    {
        private readonly double permittedGap;
        
        public ResultEntryCompletionStrategy(double permittedGap)
        {
            if (!(permittedGap >= 0))
            {
                throw new ArgumentException("Must be non-negative", nameof(permittedGap));
            }

            this.permittedGap = permittedGap;
        }

        public bool CanContinueInNextQuery(ResultEntry? entry)
        {
            return entry != null && entry.QueryMatchStartsAt + entry.DiscreteTrackCoverageLength >= entry.QueryLength - permittedGap;
        }
    }
}