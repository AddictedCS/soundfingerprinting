namespace SoundFingerprinting.Math
{
    using System.Collections.Generic;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Utils;

    public class StandardGroupingCounter : IGroupingCounter
    {
        public IEnumerable<SubFingerprintData> GroupByAndCount(List<uint>[] results, int thresholdVotes, ISet<string> clusters, ISubFingerprintIdsToDataResolver resolver)
        {
            var ids = SubFingerprintGroupingCounter.GroupByAndCount(results, thresholdVotes);
            return resolver.ResolveFromIds(ids, clusters);
        }
    }
}
