namespace SoundFingerprinting.Query
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.Dao.Entities;

    public interface IQueryDao
    {
        IEnumerable<Tuple<SubFingerprint, int>> ReadSubFingerprintsByHashBucketsHavingThreshold(long[] buckets, int threshold);
    }
}
