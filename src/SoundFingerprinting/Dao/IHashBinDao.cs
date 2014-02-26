namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public interface IHashBinDao
    {
        void InsertHashBins(long[] hashBins, IModelReference subFingerprintReference);

        IList<HashBinData> ReadHashBinsByHashTable(int hashTableId);

        IList<HashData> ReadHashDataByTrackId(IModelReference trackReference);

        IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(
            long[] hashBuckets, int thresholdVotes);

        IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(
            long[] hashBuckets, int thresholdVotes, string trackGroupId);
    }
}