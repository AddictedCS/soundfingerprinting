namespace Soundfingerprinting.Dao
{
    using System.Collections.Generic;

    using Soundfingerprinting.Dao.Entities;
    using Soundfingerprinting.DbStorage.Entities;

    public interface IModelService
    {
        void InsertFingerprint(Fingerprint fingerprint);

        void InsertFingerprint(IEnumerable<Fingerprint> collection);

        void InsertTrack(Track track);

        void InsertTrack(IEnumerable<Track> collection);

        void InsertAlbum(Album album);

        void InsertAlbum(IEnumerable<Album> collection);

        void InsertHashBin(HashBinMinHash hashBin);

        void InsertHashBin(IEnumerable<HashBinMinHash> collection);

        IDictionary<Track, int> ReadDuplicatedTracks();

        IList<Fingerprint> ReadFingerprints();

        IList<Fingerprint> ReadFingerprintsByTrackId(int trackId, int numberOfFingerprintsToRead);

        IDictionary<int, IList<Fingerprint>> ReadFingerprintsByMultipleTrackId(
            IEnumerable<Track> tracks, int numberOfFingerprintsToRead);

        Fingerprint ReadFingerprintById(int id);

        IList<Fingerprint> ReadFingerprintById(IEnumerable<int> ids);

        IList<Track> ReadTracks();

        Track ReadTrackById(int id);

        Track ReadTrackByArtistAndTitleName(string artist, string title);

        IList<Album> ReadAlbums();

        Album ReadUnknownAlbum();

        Album ReadAlbumByName(string name);

        Album ReadAlbumById(int id);

        IList<Track> ReadTrackByFingerprint(int id);

        IDictionary<int, IList<HashBinMinHash>> ReadFingerprintsByHashBucketLsh(long[] hashBucket);

        int DeleteTrack(int trackId);

        int DeleteTrack(Track track);

        int DeleteTrack(IEnumerable<int> collection);

        int DeleteTrack(IEnumerable<Track> collection);
    }
}