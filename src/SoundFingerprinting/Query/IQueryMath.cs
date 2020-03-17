namespace SoundFingerprinting.Query
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;

    public interface IQueryMath
    {
        List<ResultEntry> GetBestCandidates(GroupedQueryResults groupedQueryResults, int maxNumberOfMatchesToReturn, IModelService modelService, QueryConfiguration queryConfiguration);
    }
}