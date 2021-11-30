namespace SoundFingerprinting.Query
{
    using System;

    /// <summary>
    ///  A strategy that decides whether a result entry can continue in the next streaming query <see cref="StatefulRealtimeResultEntryAggregator"/>.
    /// </summary>
    internal sealed class ResultEntryCompletionStrategy : ICompletionStrategy<ResultEntry>
    {
        private readonly double permittedGap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultEntryCompletionStrategy"/> class.
        /// </summary>
        /// <param name="permittedGap">Permitted gap used to ignore gaps of a certain length.</param>
        /// <exception cref="ArgumentException"><paramref name="permittedGap"/> should be bigger or equal than zero.</exception>
        public ResultEntryCompletionStrategy(double permittedGap)
        {
            if (!(permittedGap >= 0))
            {
                throw new ArgumentException("Must be non-negative", nameof(permittedGap));
            }

            this.permittedGap = permittedGap;
        }

        /// <inheritdoc cref="ICompletionStrategy{T}.CanContinueInNextQuery" />
        public bool CanContinueInNextQuery(ResultEntry? entry)
        {
            return entry != null && entry.Track.Length >= entry.QueryLength - entry.QueryMatchStartsAt - permittedGap;
        }
    }
}