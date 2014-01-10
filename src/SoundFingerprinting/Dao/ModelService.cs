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

        private readonly PermutationsDao permutationsDao;

        public ModelService()
            : this(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>())
        {
        }

        public ModelService(IDatabaseProviderFactory databaseProviderFactory, IModelBinderFactory modelBinderFactory)
        {
            trackDao = new TrackDao(databaseProviderFactory, modelBinderFactory);
            hashBinDao = new HashBinDao(databaseProviderFactory, modelBinderFactory);
            subFingerprintDao = new SubFingerprintDao(databaseProviderFactory, modelBinderFactory);
            permutationsDao = new PermutationsDao(databaseProviderFactory, modelBinderFactory);
        }

        public IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] buckets, int threshold)
        {
            return hashBinDao.ReadSubFingerprintDataByHashBucketsWithThreshold(buckets, threshold).ToList();
        }

        public int[][] ReadPermutationsForLSHAlgorithm()
        {
            return permutationsDao.ReadPermutationsForLSHAlgorithm();
        }

        public ITrackReference InsertTrack(TrackData track)
        {
            return new RDBMSTrackReference(trackDao.Insert(track));
        }

        public void InsertHashDataForTrack(byte[] subFingerprintSignature, long[] hashBuckets, ITrackReference trackReference)
        {
            if (!(trackReference is RDBMSTrackReference))
            {
                throw new NotSupportedException("Cannot insert non relational reference to relational database");
            }

            long subFingerprintId = subFingerprintDao.Insert(subFingerprintSignature, ((RDBMSTrackReference)trackReference).Id);
            hashBinDao.Insert(hashBuckets, subFingerprintId);
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
