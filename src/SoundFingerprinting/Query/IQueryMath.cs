namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;

    internal interface IQueryMath
    {
        List<ResultEntry> GetBestCandidates(GroupedQueryResults groupedQueryResults, int maxNumberOfMatchesToReturn, IModelService modelService, QueryConfiguration queryConfiguration);
    }
}