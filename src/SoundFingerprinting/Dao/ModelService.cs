namespace SoundFingerprinting.Dao
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Dao.SQL;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    public class ModelService : IModelService
    {
        private readonly TrackDao trackDao;

        private readonly HashBinDao hashBinDao;

        private readonly SubFingerprintDao subFingerprintDao;

        private readonly FingerprintDao fingerprintDao;

        public ModelService()
            : this(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>())
        {
        }

        public ModelService(IDatabaseProviderFactory databaseProviderFactory, IModelBinderFactory modelBinderFactory)
        {
            trackDao = new TrackDao(databaseProviderFactory, modelBinderFactory);
            hashBinDao = new HashBinDao(databaseProviderFactory, modelBinderFactory);
            subFingerprintDao = new SubFingerprintDao(databaseProviderFactory, modelBinderFactory);
            fingerprintDao = new FingerprintDao(databaseProviderFactory, modelBinderFactory);
        }

        public IList<SubFingerprintData> ReadSubFingerprintDataByHashBucketsWithThreshold(long[] buckets, int threshold)
        {
            return hashBinDao.ReadSubFingerprintDataByHashBucketsWithThreshold(buckets, threshold).ToList();
        }

        public IModelReference InsertFingerprint(FingerprintData fingerprintData)
        {
            if (!(fingerprintData.TrackReference is ModelReference<int>))
            {
                throw new NotSupportedException("Cannot insert a non relational reference to relational database");
            }

            int fingerprintId = fingerprintDao.Insert(fingerprintData.Signature, ((ModelReference<int>)fingerprintData.TrackReference).Id);
            return fingerprintData.FingerprintReference = new ModelReference<int>(fingerprintId);
        }

        public IModelReference InsertTrack(TrackData track)
        {
            trackDao.Insert(track);
            return track.TrackReference;
        }

        public void InsertHashDataForTrack(IEnumerable<HashData> hashes, IModelReference trackReference)
        {
            if (!(trackReference is ModelReference<int>))
            {
                throw new NotSupportedException("Cannot insert non relational reference to relational database");
            }

            foreach (var hashData in hashes)
            {
                long subFingerprintId = subFingerprintDao.Insert(
                    hashData.SubFingerprint, ((ModelReference<int>)trackReference).Id);
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

        public IList<FingerprintData> ReadFingerprintsByTrackReference(IModelReference trackReference)
        {
            if (!(trackReference is ModelReference<int>))
            {
                throw new NotSupportedException("Cannot read non relational data from relational database");
            }

            return fingerprintDao.ReadFingerprintsByTrackId(((ModelReference<int>)trackReference).Id);
        }

        public TrackData ReadTrackByReference(IModelReference trackReference)
        {
            if (!(trackReference is ModelReference<int>))
            {
                throw new NotSupportedException("Cannot read a non relational reference from relational database");
            }

            return trackDao.ReadById(((ModelReference<int>)trackReference).Id);
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            return trackDao.ReadTrackByISRC(isrc);
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            if (!(trackReference is ModelReference<int>))
            {
                throw new NotSupportedException("Cannot delete a non relational reference from relational database");
            }

            return trackDao.DeleteTrack(((ModelReference<int>)trackReference).Id);
        }
    }
}
