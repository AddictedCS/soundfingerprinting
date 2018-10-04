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
        protected readonly ITrackDao TrackDao;
        protected readonly ISubFingerprintDao SubFingerprintDao;

        protected ModelService(ITrackDao trackDao, ISubFingerprintDao subFingerprintDao)
        {
            TrackDao = trackDao;
            SubFingerprintDao = subFingerprintDao;
        }

        public ModelServiceInfo Info
        {
            get
            {
                return new ModelServiceInfo(TrackDao.Count, SubFingerprintDao.SubFingerprintsCount, SubFingerprintDao.HashCountsPerTable.ToArray());
            }
        }

        public virtual IModelReference Insert(TrackInfo trackInfo, IEnumerable<HashedFingerprint> hashedFingerprints)
        {
            var fingerprints = hashedFingerprints.ToList();
            if (!fingerprints.Any())
            {
                return ModelReference<int>.Null;
            }

            var trackReference = TrackDao.InsertTrack(trackInfo).TrackReference;
            SubFingerprintDao.InsertHashDataForTrack(fingerprints, trackReference);
            return trackReference;
        }

        public virtual FingerprintsQueryResponse ReadSubFingerprints(IEnumerable<QueryHash> hashes, QueryConfiguration config)
        {
            var queryHashes = hashes.ToList();
            if (!queryHashes.Any())
            {
                return FingerprintsQueryResponse.Empty;
            }

            return SubFingerprintDao.ReadSubFingerprints(queryHashes, config);
        }

        public virtual bool ContainsTrack(string isrc, string artist, string title)
        {
            if (!string.IsNullOrEmpty(isrc))
            {
                return ReadTrackById(isrc) != null;
            }

            return ReadTrackByTitle(title).Any();
        }

        public virtual IEnumerable<TrackData> ReadAllTracks()
        {
            return TrackDao.ReadAll();
        }

        public virtual IEnumerable<TrackData> ReadTrackByTitle(string title)
        {
            return TrackDao.ReadTrackByTitle(title);
        }

        public virtual IEnumerable<TrackData> ReadTracksByReferences(params IModelReference[] ids)
        {
            return TrackDao.ReadTracks(ids);
        }

        public virtual TrackData ReadTrackById(string id)
        {
            return TrackDao.ReadTrackById(id);
        }

        public virtual int DeleteTrack(IModelReference trackReference)
        {
            return TrackDao.DeleteTrack(trackReference);
        }
    }
}
