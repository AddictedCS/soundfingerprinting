namespace SoundFingerprinting.Math
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;

    public interface IGroupingCounter
    {
        IEnumerable<SubFingerprintData> GroupByAndCount(List<uint>[] results, int thresholdVotes, ISubFingerprintIdsToDataResolver resolver);
    }
}
