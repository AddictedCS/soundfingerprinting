namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;

    using DAO;
    using DAO.Data;

    public interface IRAMStorage
    {
        IDictionary<IModelReference, TrackData> Tracks { get; }                         // key: track reference

        int SubFingerprintsCount { get; }

        IEnumerable<int> HashCountsPerTable { get; }

        void AddSubFingerprint(SubFingerprintData subFingerprintData);

        List<uint> GetSubFingerprintsByHashTableAndHash(int table, int hash);

        void AddSpectralImages(IEnumerable<SpectralImageData> spectralImages);

        IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference);

        void Snapshot(string path);

        TrackData AddTrack(TrackData track);

        int DeleteTrack(IModelReference trackReference);

        SubFingerprintData ReadSubFingerprintById(uint id);

        IEnumerable<SubFingerprintData> ReadSubFingerprintByTrackReference(IModelReference trackReference);
        
        int DeleteSubFingerprintsByTrackReference(IModelReference trackReference);
    }
}