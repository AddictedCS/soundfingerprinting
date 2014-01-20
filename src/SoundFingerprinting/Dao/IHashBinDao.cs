namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public interface IHashBinDao
    {
        void Insert(long[] hashBins, long subFingerprintId);

        IList<HashBinData> ReadHashBinsByHashTable(int hashTableId);

        IList<HashData> ReadHashDataByTrackId(int trackId);

        IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(
            long[] hashBuckets, int thresholdVotes);
    }
}