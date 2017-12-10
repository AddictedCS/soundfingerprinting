namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;

    using DAO;
    using DAO.Data;
    using Data;

    internal interface IRAMStorage
    {
        IDictionary<int, TrackData> Tracks { get; }                         // key: track reference

        void AddSubfingerprint(HashedFingerprint hashedFingerprint, IModelReference trackReference);

        List<ulong> GetSubFingerprintsByHashTableAndHash(int table, int hash);

        void AddSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference);

        IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference);

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