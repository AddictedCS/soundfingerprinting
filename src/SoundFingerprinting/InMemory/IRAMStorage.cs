namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;

    using DAO;
    using DAO.Data;
    using Data;

    public interface IRAMStorage
    {
        IDictionary<IModelReference, TrackData> Tracks { get; }                         // key: track reference

        int SubFingerprintsCount { get; }

        IEnumerable<int> HashCountsPerTable { get; }

        SubFingerprintData AddHashedFingerprint(HashedFingerprint hashedFingerprint, IModelReference trackReference);

        void AddSubFingerprint(SubFingerprintData subFingerprintData);

        List<uint> GetSubFingerprintsByHashTableAndHash(int table, int hash);

        void AddSpectralImages(IEnumerable<float[]> spectralImages, IModelReference trackReference);

        IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference);

        void InitializeFromFile(string path);

        void Snapshot(string path);

        TrackData AddTrack(TrackInfo track, double durationInSeconds);

        TrackData AddTrack(TrackData track);

        int DeleteTrack(IModelReference trackReference);

        SubFingerprintData ReadSubFingerprintById(uint id);

        IEnumerable<SubFingerprintData> ReadSubFingerprintByTrackReference(IModelReference trackReference);
        
        int DeleteSubFingerprintsByTrackReference(IModelReference trackReference);
    }
}