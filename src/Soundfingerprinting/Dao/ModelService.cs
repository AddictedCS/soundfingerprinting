namespace Soundfingerprinting.Dao
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Soundfingerprinting.Dao.Entities;
    using Soundfingerprinting.Dao.Internal;
    using Soundfingerprinting.Infrastructure;

    public class ModelService : IModelService
    {
        private readonly AlbumDao albumDao;

        private readonly FingerprintDao fingerprintDao;

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
            albumDao = new AlbumDao(databaseProviderFactory, modelBinderFactory);
            trackDao = new TrackDao(databaseProviderFactory, modelBinderFactory);
            fingerprintDao = new FingerprintDao(databaseProviderFactory, modelBinderFactory);
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

        public void InsertFingerprint(Fingerprint fingerprint)
        {
            fingerprintDao.Insert(fingerprint);
        }

        public void InsertFingerprint(IEnumerable<Fingerprint> collection)
        {
            fingerprintDao.Insert(collection);
        }

        public void InsertTrack(Track track)
        {
            trackDao.Insert(track);
        }

        public void InsertTrack(IEnumerable<Track> collection)
        {
            trackDao.Insert(collection);
        }

        public void InsertAlbum(Album album)
        {
            albumDao.Insert(album);
        }

        public virtual void InsertAlbum(IEnumerable<Album> collection)
        {
            albumDao.Insert(collection);
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

        public IEnumerable<HashBinMinHash> ReadAll()
        {
            return hashBinMinHashDao.ReadAll();
        }

        public IEnumerable<Tuple<SubFingerprint, int>> ReadSubFingerprintsByHashBucketsHavingThreshold(long[] buckets, int threshold)
        {
            return hashBinMinHashDao.ReadSubFingerprintsByHashBucketsHavingThreshold(buckets, threshold);
        }

        public IDictionary<Track, int> ReadDuplicatedTracks()
        {
            return trackDao.ReadDuplicatedTracks();
        }

        public IList<Fingerprint> ReadFingerprints()
        {
            return fingerprintDao.Read();
        }

        public IList<Fingerprint> ReadFingerprintsByTrackId(int trackId, int numberOfFingerprintsToRead)
        {
            return fingerprintDao.ReadFingerprintsByTrackId(trackId, numberOfFingerprintsToRead);
        }

        public IDictionary<int, IList<Fingerprint>> ReadFingerprintsByMultipleTrackId(
            IEnumerable<Track> tracks, int numberOfFingerprintsToRead)
        {
            return fingerprintDao.ReadFingerprintsByMultipleTrackId(tracks, numberOfFingerprintsToRead);
        }

        public Fingerprint ReadFingerprintById(int id)
        {
            return fingerprintDao.ReadById(id);
        }

        public IList<Fingerprint> ReadFingerprintById(IEnumerable<int> ids)
        {
            return fingerprintDao.ReadById(ids);
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

        public IList<Album> ReadAlbums()
        {
            return albumDao.Read();
        }

        public Album ReadUnknownAlbum()
        {
            return albumDao.ReadUnknownAlbum();
        }

        public Album ReadAlbumByName(string name)
        {
            return albumDao.ReadAlbumByName(name);
        }

        public Album ReadAlbumById(int id)
        {
            return albumDao.ReadById(id);
        }

        public IList<Track> ReadTrackByFingerprint(int id)
        {
            return trackDao.ReadTrackByFingerprintId(id);
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