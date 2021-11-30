namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public class AVRAMStorage : IRAMStorage
    {
        private IRAMStorage audio;
        private IRAMStorage video;
        
        public AVRAMStorage()
        {
            
        }
        
        public int TracksCount { get; }
        public int SubFingerprintsCount { get; }
        public IEnumerable<int> HashCountsPerTable { get; }
        public List<uint> GetSubFingerprintsByHashTableAndHash(int table, int hash, MediaType mediaType)
        {
            throw new System.NotImplementedException();
        }

        public void Snapshot(string path)
        {
            throw new System.NotImplementedException();
        }

        public void InsertTrack(TrackInfo track, AVHashes hashes)
        {
            throw new System.NotImplementedException();
        }

        public int DeleteTrack(IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetTrackByReference(IModelReference trackReference, out TrackData track)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<TrackData> SearchByTitle(string title)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> GetTrackIds()
        {
            throw new System.NotImplementedException();
        }

        public TrackData? ReadByTrackId(string id)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<SubFingerprintData> ReadSubFingerprintsByUid(IEnumerable<uint> ids, MediaType mediaType)
        {
            throw new System.NotImplementedException();
        }

        public AVHashes ReadAvHashesByTrackId(string trackId)
        {
            throw new System.NotImplementedException();
        }

        public void AddSpectralImages(IModelReference trackReference, IEnumerable<float[]> images)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference)
        {
            throw new System.NotImplementedException();
        }
    }
}