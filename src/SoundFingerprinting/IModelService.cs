namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface IModelService
    {
        IModelReference InsertFingerprint(FingerprintData fingerprint);

        IModelReference InsertTrack(TrackData track);

        void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference);

        IList<HashedFingerprint> ReadHashedFingerprintsByTrack(IModelReference trackReference);
            
        IList<TrackData> ReadAllTracks();

        IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title);

        IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference);
            
        TrackData ReadTrackByReference(IModelReference trackReference);

        TrackData ReadTrackByISRC(string isrc);

        int DeleteTrack(IModelReference trackReference);

        IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] buckets, int threshold);

        IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsThresholdWithGroupId(long[] buckets, int threshold, string trackGroupId);

        void InsertSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference);

        List<SpectralImageData> GetSpectralImagesByTrackId(IModelReference trackReference);

        ISet<SubFingerprintData> ReadAllSubFingerprintCandidatesWithThreshold(IEnumerable<HashedFingerprint> hashes, int threshold);
    }
}
