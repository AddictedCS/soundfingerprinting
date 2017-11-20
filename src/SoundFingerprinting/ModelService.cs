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

        public abstract bool SupportsBatchedSubFingerprintQuery { get; }

        public virtual IList<SubFingerprintData> ReadSubFingerprints(int[] hashBins, QueryConfiguration config)
        {
            return subFingerprintDao.ReadSubFingerprints(hashBins, config.ThresholdVotes, config.Clusters).ToList();
        }

        public virtual ISet<SubFingerprintData> ReadSubFingerprints(IEnumerable<int[]> hashes, QueryConfiguration config)
        {
            return subFingerprintDao.ReadSubFingerprints(hashes, config.ThresholdVotes, config.Clusters);
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

        public virtual IList<HashedFingerprint> ReadHashedFingerprintsByTrack(IModelReference trackReference)
        {
            return subFingerprintDao.ReadHashedFingerprintsByTrackReference(trackReference);
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
