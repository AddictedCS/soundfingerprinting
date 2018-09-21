namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public abstract class ModelService : IModelService
    {
        private readonly ITrackDao trackDao;
        private readonly ISubFingerprintDao subFingerprintDao;

        protected ModelService(ITrackDao trackDao, ISubFingerprintDao subFingerprintDao)
        {
            this.trackDao = trackDao;
            this.subFingerprintDao = subFingerprintDao;
        }

        public virtual TrackData Insert(TrackInfo trackInfo, IEnumerable<HashedFingerprint> hashedFingerprints)
        {
            var track = new TrackData(trackInfo.Isrc, trackInfo.Artist, trackInfo.Title, string.Empty, 0, trackInfo.DurationInSeconds);
            var trackReference = trackDao.InsertTrack(track);
            subFingerprintDao.InsertHashDataForTrack(hashedFingerprints, trackReference);
            return track;
        }

        public virtual FingerprintsQueryResponse ReadSubFingerprints(IEnumerable<HashInfo> hashes, QueryConfiguration config)
        {
            return subFingerprintDao.ReadSubFingerprints(hashes, config);
        }

        public virtual bool ContainsTrack(string isrc, string artist, string title)
        {
            if (!string.IsNullOrEmpty(isrc))
            {
                return ReadTrackByISRC(isrc) != null;
            }

            return ReadTrackByArtistAndTitleName(artist, title).Any();
        }

        public virtual IModelReference InsertTrack(TrackData track)
        {
            return trackDao.InsertTrack(track);
        }

        public virtual void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashes, IModelReference trackReference)
        {
            subFingerprintDao.InsertHashDataForTrack(hashes, trackReference);
        }

        public virtual IList<TrackData> ReadAllTracks()
        {
            return trackDao.ReadAll();
        }

        public virtual IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            return trackDao.ReadTrackByArtistAndTitleName(artist, title);
        }

        public virtual TrackData ReadTrackByReference(IModelReference trackReference)
        {
            return trackDao.ReadTrack(trackReference);
        }

        public virtual List<TrackData> ReadTracksByReferences(IEnumerable<IModelReference> ids)
        {
            return trackDao.ReadTracks(ids);
        }

        public virtual TrackData ReadTrackByISRC(string isrc)
        {
            return trackDao.ReadTrackByISRC(isrc);
        }

        public virtual int DeleteTrack(IModelReference trackReference)
        {
            return trackDao.DeleteTrack(trackReference);
        }
    }
}
