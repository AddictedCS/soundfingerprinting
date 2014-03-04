namespace SoundFingerprinting.DAO
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public interface IHashBinDao
    {
        void InsertHashBins(long[] hashBins, IModelReference subFingerprintReference, IModelReference trackReference);

        IList<HashData> ReadHashDataByTrackReference(IModelReference trackReference);

        IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(
            long[] hashBins, int thresholdVotes);

        IEnumerable<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(
            long[] hashBuckets, int thresholdVotes, string trackGroupId);
    }
}