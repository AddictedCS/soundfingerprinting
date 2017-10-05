namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;

    using DAO;
    using DAO.Data;
    using Data;

    internal interface IRAMStorage
    {
        IDictionary<int, TrackData> Tracks { get; }                         // key: track reference

        IDictionary<IModelReference, List<FingerprintData>> Fingerprints { get; }       // key: track reference

        IDictionary<IModelReference, List<SpectralImageData>> SpectralImages { get; }   // key: track reference

        void AddSubfingerprint(HashedFingerprint hashedFingerprint, IModelReference trackReference);

        IEnumerable<ulong> GetSubFingerprintsByHashTableAndHash(int table, long hash);

        int NumberOfHashTables { get; }

        void Reset(int numberOfHashTables);

        void InitializeFromFile(string path);

        void Snapshot(string path);

        IModelReference AddTrack(TrackData track);

        int DeleteTrack(IModelReference trackReference);

        SubFingerprintData ReadSubFingerprintById(ulong id);

        IEnumerable<SubFingerprintData> ReadSubFingerprintByTrackReference(IModelReference trackReference);
    }
}