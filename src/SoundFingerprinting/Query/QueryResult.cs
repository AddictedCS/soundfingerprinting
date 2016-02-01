namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;
    using System.Linq;

    public class QueryResult
    {
        /// <summary>
        /// Gets a value indicating whether query result contains any matches
        /// </summary>
        public bool IsSuccessful
        {
            get
            {
                return ResultEntries != null && ResultEntries.Any();
            }
        }

        /// <summary>
        /// Gets additional information about the query
        /// </summary>
        public QueryInfo Info { get; internal set; }

        /// <summary>
        /// Gets the list of potential matches, sorted from the most probable to the least probable
        /// </summary>
        public List<ResultEntry> ResultEntries { get; internal set; }

        /// <summary>
        /// Gets the number of analyzed tracks. This information is useful for debugging and analysis purposes
        /// </summary>
        public int AnalyzedTracksCount { get; internal set; }

        /// <summary>
        /// Gets best match if any
        /// </summary>
        public ResultEntry BestMatch
        {
            get
            {
                if (IsSuccessful)
                {
                    return ResultEntries[0];
                }

                return null;
            }
        }
    }
}