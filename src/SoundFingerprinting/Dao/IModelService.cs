namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public interface IModelService
    {
        ITrackReference InsertTrack(TrackData track);

        void InsertHashDataForTrack(byte[] subFingerprintSignature, long[] hashBuckets, ITrackReference trackReference);

        IList<TrackData> ReadAllTracks();

        IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title);

        TrackData ReadTrackByReference(ITrackReference trackReference);

        TrackData ReadTrackByISRC(string isrc);

        int DeleteTrack(ITrackReference trackReference);

        IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] buckets, int threshold);

        int[][] ReadPermutationsForLSHAlgorithm();
    }
}
