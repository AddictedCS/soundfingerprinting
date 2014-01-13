namespace SoundFingerprinting.Dao.Internal
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    internal interface IHashBinDao
    {
        void Insert(long[] hashBins, long subFingerprintId);

        IList<HashBinData> ReadHashBinsByHashTable(int hashTableId);

        IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] hashBuckets, int thresholdVotes);
    }
}