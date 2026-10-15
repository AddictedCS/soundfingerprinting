namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;

    internal interface IQueryMath
    {
        /// <summary>
        ///  Builds the result entries of the already retrieved tracks. Pure computation: no storage access.
        /// </summary>
        /// <param name="groupedQueryResults">Grouped candidates of the query.</param>
        /// <param name="tracks">Tracks selected from the candidates, read from the storage by the caller.</param>
        /// <param name="queryConfiguration">Query configuration.</param>
        /// <returns>Result entries ordered by descending score.</returns>
        List<ResultEntry> GetBestCandidates(GroupedQueryResults groupedQueryResults, IReadOnlyList<TrackData> tracks, QueryConfiguration queryConfiguration);
    }
}
