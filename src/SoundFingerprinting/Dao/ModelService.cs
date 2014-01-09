namespace SoundFingerprinting.Dao
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Dao.Internal;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Infrastructure;

    public class ModelService : IModelService
    {
        private readonly TrackDao trackDao;

        private readonly HashBinMinHashDao hashBinMinHashDao;

        private readonly SubFingerprintDao subFingerprintDao;

        private readonly PermutationsDao permutationsDao;

        public ModelService()
            : this(DependencyResolver.Current.Get<IDatabaseProviderFactory>(), DependencyResolver.Current.Get<IModelBinderFactory>())
        {
        }

        public ModelService(IDatabaseProviderFactory databaseProviderFactory, IModelBinderFactory modelBinderFactory)
        {
            trackDao = new TrackDao(databaseProviderFactory, modelBinderFactory);
            hashBinMinHashDao = new HashBinMinHashDao(databaseProviderFactory, modelBinderFactory);
            subFingerprintDao = new SubFingerprintDao(databaseProviderFactory, modelBinderFactory);
            permutationsDao = new PermutationsDao(databaseProviderFactory, modelBinderFactory);
        }

        public void InsertSubFingerprint(SubFingerprint subFingerprint)
        {
            subFingerprintDao.Insert(subFingerprint);
        }

        public void InsertSubFingerprint(IEnumerable<SubFingerprint> subFingerprints)
        {
            subFingerprintDao.Insert(subFingerprints);
        }

        public int[][] ReadPermutationsForLSHAlgorithm()
        {
            return permutationsDao.ReadPermutationsForLSHAlgorithm();
        }

        public void InsertHashData(HashData hashData)
        {
            throw new NotImplementedException();
        }

        public ITrackReference InsertTrackData(TrackData track)
        {
            throw new NotImplementedException();
        }

        public void InsertTrack(Track track)
        {
            trackDao.Insert(track);
        }

        public void InsertTrack(IEnumerable<Track> collection)
        {
            trackDao.Insert(collection);
        }

        public void InsertHashBin(HashBinMinHash hashBin)
        {
            hashBinMinHashDao.Insert(hashBin);
        }

        public void InsertHashBin(IEnumerable<HashBinMinHash> collection)
        {
            foreach (var hashBinMinHash in collection)
            {
                InsertHashBin(hashBinMinHash);
            }
        }

        public IEnumerable<Tuple<SubFingerprint, int>> ReadSubFingerprintsByHashBucketsHavingThreshold(long[] buckets, int threshold)
        {
            return hashBinMinHashDao.ReadSubFingerprintsByHashBucketsHavingThreshold(buckets, threshold);
        }

        public virtual IList<Track> ReadTracks()
        {
            return trackDao.Read();
        }

        public Track ReadTrackById(int id)
        {
            return trackDao.ReadById(id);
        }

        public Track ReadTrackByArtistAndTitleName(string artist, string title)
        {
            return trackDao.ReadTrackByArtistAndTitleName(artist, title);
        }

        public Track ReadTrackByISRC(string isrc)
        {
            return trackDao.ReadTrackByISRC(isrc);
        }

        public int DeleteTrack(int trackId)
        {
            return trackDao.DeleteTrack(trackId);
        }

        public int DeleteTrack(Track track)
        {
            return DeleteTrack(track.Id);
        }

        public int DeleteTrack(IEnumerable<int> collection)
        {
            return collection.Sum(trackId => trackDao.DeleteTrack(trackId));
        }

        public int DeleteTrack(IEnumerable<Track> collection)
        {
            return DeleteTrack(collection.Select(track => track.Id));
        }
    }
}
