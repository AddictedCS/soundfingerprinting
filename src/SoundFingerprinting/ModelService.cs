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
        protected readonly string Id;
        
        protected ModelService(string id, ITrackDao trackDao, ISubFingerprintDao subFingerprintDao)
        {
            Id = id;
            TrackDao = trackDao;
            SubFingerprintDao = subFingerprintDao;
        }

        public virtual IEnumerable<ModelServiceInfo> Info => new[] { new ModelServiceInfo(Id, TrackDao.Count, SubFingerprintDao.SubFingerprintsCount, SubFingerprintDao.HashCountsPerTable.ToArray()) };

        protected ITrackDao TrackDao { get; }
        
        protected ISubFingerprintDao SubFingerprintDao { get; }

        public virtual void Insert(TrackInfo trackInfo, Hashes hashes)
        {
            var fingerprints = hashes.ToList();
            if (!fingerprints.Any())
            {
                return;
            }

            var trackReference = TrackDao.InsertTrack(trackInfo, hashes.DurationInSeconds).TrackReference;
            SubFingerprintDao.InsertHashDataForTrack(fingerprints, trackReference);
        }

        public virtual IEnumerable<SubFingerprintData> Query(IEnumerable<int[]> hashes, QueryConfiguration config)
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

        public virtual IEnumerable<TrackData> ReadTracksByReferences(IEnumerable<IModelReference> references)
        {
            return TrackDao.ReadTracksByReferences(references);
        }

        public virtual TrackData ReadTrackById(string trackId)
        {
            return TrackDao.ReadTrackById(trackId);
        }

        public virtual int DeleteTrack(string trackId)
        {
            var track = ReadTrackById(trackId);
            if (track == null)
            {
                return 0;
            }

            var trackReference = track.TrackReference;
            return SubFingerprintDao.DeleteSubFingerprintsByTrackReference(trackReference) + TrackDao.DeleteTrack(trackReference);
        }
    }
}
