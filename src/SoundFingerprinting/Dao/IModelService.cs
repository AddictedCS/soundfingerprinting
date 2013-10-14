namespace SoundFingerprinting.Dao
{
    using System;
    using System.Collections.Generic;

    using SoundFingerprinting.Dao.Entities;

    public interface IModelService
    {
        void InsertFingerprint(Fingerprint fingerprint);

        void InsertFingerprint(IEnumerable<Fingerprint> collection);

        void InsertTrack(Track track);

        void InsertTrack(IEnumerable<Track> collection);

        void InsertHashBin(HashBinMinHash hashBin);

        void InsertHashBin(IEnumerable<HashBinMinHash> collection);

        IList<Fingerprint> ReadFingerprints();

        IList<Fingerprint> ReadFingerprintsByTrackId(int trackId, int numberOfFingerprintsToRead);

        IDictionary<int, IList<Fingerprint>> ReadFingerprintsByMultipleTrackId(
            IEnumerable<Track> tracks, int numberOfFingerprintsToRead);

        Fingerprint ReadFingerprintById(int id);

        IList<Fingerprint> ReadFingerprintById(IEnumerable<int> ids);

        IList<Track> ReadTracks();

        Track ReadTrackById(int id);

        Track ReadTrackByArtistAndTitleName(string artist, string title);

        IList<Track> ReadTrackByFingerprint(int id);

        int DeleteTrack(int trackId);

        int DeleteTrack(Track track);

        int DeleteTrack(IEnumerable<int> collection);

        int DeleteTrack(IEnumerable<Track> collection);

        IEnumerable<HashBinMinHash> ReadAll();

        IEnumerable<Tuple<SubFingerprint, int>> ReadSubFingerprintsByHashBucketsHavingThreshold(long[] buckets, int threshold);

        void InsertSubFingerprint(SubFingerprint subFingerprint);

        void InsertSubFingerprint(IEnumerable<SubFingerprint> subFingerprints);

        int[][] ReadPermutationsForLSHAlgorithm();
    }
}