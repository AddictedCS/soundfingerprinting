namespace SoundFingerprinting.Dao
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Dao.Internal;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    public class ModelService : IModelService
    {
        private readonly TrackDao trackDao;

        private readonly HashBinDao hashBinDao;

        private readonly SubFingerprintDao subFingerprintDao;

        public ModelService()
            : this(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>())
        {
        }

        public ModelService(IDatabaseProviderFactory databaseProviderFactory, IModelBinderFactory modelBinderFactory)
        {
            trackDao = new TrackDao(databaseProviderFactory, modelBinderFactory);
            hashBinDao = new HashBinDao(databaseProviderFactory, modelBinderFactory);
            subFingerprintDao = new SubFingerprintDao(databaseProviderFactory, modelBinderFactory);
        }

        public IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] buckets, int threshold)
        {
            return hashBinDao.ReadSubFingerprintDataByHashBucketsWithThreshold(buckets, threshold).ToList();
        }

        public ITrackReference InsertTrack(TrackData track)
        {
            trackDao.Insert(track);
            return track.TrackReference;
        }

        public void InsertHashDataForTrack(IEnumerable<HashData> hashes, ITrackReference trackReference)
        {
            if (!(trackReference is RDBMSTrackReference))
            {
                throw new NotSupportedException("Cannot insert non relational reference to relational database");
            }

            foreach (var hashData in hashes)
            {
                long subFingerprintId = subFingerprintDao.Insert(
                    hashData.SubFingerprint, ((RDBMSTrackReference)trackReference).Id);
                hashBinDao.Insert(hashData.HashBins, subFingerprintId);
            }
        }

        public IList<TrackData> ReadAllTracks()
        {
            return trackDao.ReadAll();
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            return trackDao.ReadTrackByArtistAndTitleName(artist, title);
        }

        public TrackData ReadTrackByReference(ITrackReference trackReference)
        {
            if (!(trackReference is RDBMSTrackReference))
            {
                throw new NotSupportedException("Cannot read a non relational reference from relational database");
            }

            return trackDao.ReadById(((RDBMSTrackReference)trackReference).Id);
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            return trackDao.ReadTrackByISRC(isrc);
        }

        public int DeleteTrack(ITrackReference trackReference)
        {
            if (!(trackReference is RDBMSTrackReference))
            {
                throw new NotSupportedException("Cannot delete a non relational reference from relational database");
            }

            return trackDao.DeleteTrack(((RDBMSTrackReference)trackReference).Id);
        }
    }
}
