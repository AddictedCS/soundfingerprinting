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
        protected ModelService(ITrackDao trackDao, ISubFingerprintDao subFingerprintDao)
        {
            TrackDao = trackDao;
            SubFingerprintDao = subFingerprintDao;
        }

        public virtual ModelServiceInfo Info => new ModelServiceInfo(TrackDao.Count, SubFingerprintDao.SubFingerprintsCount, SubFingerprintDao.HashCountsPerTable.ToArray());

        protected ITrackDao TrackDao { get; }
        
        protected ISubFingerprintDao SubFingerprintDao { get; }

        public virtual IModelReference Insert(TrackInfo trackInfo, IEnumerable<HashedFingerprint> hashedFingerprints)
        {
            var fingerprints = hashedFingerprints.ToList();
            if (!fingerprints.Any())
            {
                return null;
            }

            var trackReference = TrackDao.InsertTrack(trackInfo).TrackReference;
            SubFingerprintDao.InsertHashDataForTrack(fingerprints, trackReference);
            return trackReference;
        }

        public virtual IEnumerable<SubFingerprintData> ReadSubFingerprints(IEnumerable<int[]> hashes, QueryConfiguration config)
        {
            var queryHashes = hashes.ToList();
            if (!queryHashes.Any())
            {
                return Enumerable.Empty<SubFingerprintData>();
            }

            return SubFingerprintDao.ReadSubFingerprints(queryHashes, config);
        }

        public virtual IEnumerable<TrackData> ReadAllTracks()
        {
            return TrackDao.ReadAll();
        }

        public virtual IEnumerable<TrackData> ReadTrackByTitle(string title)
        {
            return TrackDao.ReadTrackByTitle(title);
        }

        public virtual TrackData ReadTrackByReference(IModelReference ids)
        {
            return TrackDao.ReadTrack(ids);
        }

        public virtual IEnumerable<TrackData> ReadTracksByReferences(IEnumerable<IModelReference> references)
        {
            return TrackDao.ReadTracksByReferences(references);
        }

        public virtual TrackData ReadTrackById(string id)
        {
            return TrackDao.ReadTrackById(id);
        }

        public virtual int DeleteTrack(IModelReference trackReference)
        {
            int deletedSubFingerprints = SubFingerprintDao.DeleteSubFingerprintsByTrackReference(trackReference);
            int deletedTrack = TrackDao.DeleteTrack(trackReference);
            return deletedSubFingerprints + deletedTrack;
        }
    }
}
