namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public interface IHashBinDao
    {
        void InsertHashBins(long[] hashBins, IModelReference subFingerprintReference);

        IList<HashData> ReadHashDataByTrackReference(IModelReference trackReference);

        IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(
            long[] hashBuckets, int thresholdVotes);

        IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(
            long[] hashBuckets, int thresholdVotes, string trackGroupId);
    }
}