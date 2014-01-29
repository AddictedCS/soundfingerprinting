namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public interface IHashBinDao
    {
        void Insert(long[] hashBins, long subFingerprintId, int trackId);

        IList<HashBinData> ReadHashBinsByHashTable(int hashTableId);

        IList<HashData> ReadHashDataByTrackId(int trackId);

        IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(
            long[] hashBuckets, int thresholdVotes);

        IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(
            long[] hashBuckets, int thresholdVotes, string trackGroupId);
    }
}