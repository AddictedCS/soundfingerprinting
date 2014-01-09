namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public interface IModelService
    {
        ITrackReference InsertTrack(TrackData track);

        void InsertHashData(HashData hashData, ITrackReference trackReference);

        IList<TrackData> ReadTracks();

        TrackData ReadTrackByReference(ITrackReference trackReference);

        TrackData ReadTrackByArtistAndTitleName(string artist, string title);

        TrackData ReadTrackByISRC(string isrc);

        int DeleteTrack(ITrackReference trackReference);

        IEnumerable<SubFingerprintData> ReadSubFingerprintsByHashBucketsHavingThreshold(long[] buckets, int threshold);

        int[][] ReadPermutationsForLSHAlgorithm();
    }
}
