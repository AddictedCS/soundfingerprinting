namespace SoundFingerprinting.Dao
{
    using System.Collections.Generic;

    using SoundFingerprinting.Data;

    public interface IModelService
    {
        IFingerprintReference InsertFingerprint(FingerprintData fingerprintData);

        ITrackReference InsertTrack(TrackData track);

        void InsertHashDataForTrack(IEnumerable<HashData> hashes, ITrackReference trackReference);

        IList<TrackData> ReadAllTracks();

        IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title);

        IList<FingerprintData> ReadFingerprintsByTrackReference(ITrackReference trackReference);
            
        TrackData ReadTrackByReference(ITrackReference trackReference);

        TrackData ReadTrackByISRC(string isrc);

        int DeleteTrack(ITrackReference trackReference);

        IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] buckets, int threshold);
    }
}
