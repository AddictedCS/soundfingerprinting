namespace SoundFingerprinting.InMemory
{
    using System.Collections.Generic;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface IRAMStorage
    {
        /// <summary>
        ///  Gets total number of tracks in the storage.
        /// </summary>
        int TracksCount { get; }
        
        /// <summary>
        ///  Gets total number of sub-fingerprints in the storage.
        /// </summary>
        int SubFingerprintsCount { get; }

        /// <summary>
        ///  Gets statistical info about model service.
        /// </summary>
        IEnumerable<ModelServiceInfo> Info { get; }

        /// <summary>
        ///  Gets list of sub-fingerprints by hash-table and hash bin.
        /// </summary>
        /// <param name="table">Index of the hashtable to look at.</param>
        /// <param name="hash">Hash bin to lookup.</param>
        /// <param name="mediaType">Hash bin of a certain media type to lookup.</param>
        /// <returns>List of sub-fingerprints.</returns>
        List<uint> GetSubFingerprintsByHashTableAndHash(int table, int hash, MediaType mediaType);

        /// <summary>
        ///  Snapshot current ram storage to be able to reload it later.
        /// </summary>
        /// <param name="path">Path to file to snapshot it.</param>
        /// <remarks>
        ///  Even though RAM storage is not supposed to provide persistence, sharpshooting was added to be able to test storages outside of the same process.
        /// </remarks>
        void Snapshot(string path);

        /// <summary>
        ///  Inserts a new track into the storage.
        /// </summary>
        /// <param name="track">Track to store.</param>
        /// <param name="hashes">Associated hashes to save.</param>
        void InsertTrack(TrackInfo track, AVHashes hashes);

        /// <summary>
        ///  Delete track by track reference.
        /// </summary>
        /// <param name="trackReference">Track reference to remove.</param>
        /// <returns>Number of modified rows.</returns>
        int DeleteTrack(IModelReference trackReference);

        /// <summary>
        /// Try get track by track reference.
        /// </summary>
        /// <param name="trackReference">Track reference.</param>
        /// <param name="track">If found an instance of <see cref="TrackData"/> will be returned.</param>
        /// <returns>True if found, otherwise false.</returns>
        bool TryGetTrackByReference(IModelReference trackReference, out TrackData track);

        /// <summary>
        ///  Get all tracks IDs.
        /// </summary>
        /// <returns>All track ids.</returns>
        IEnumerable<string> GetTrackIds();
        
        /// <summary>
        ///  Read track by ID.
        /// </summary>
        /// <param name="id">ID to read.</param>
        /// <returns>An instance of <see cref="TrackData"/> or null.</returns>
        TrackData? ReadByTrackId(string id);

        /// <summary>
        ///  Reads sub-fingerprints by ID and media type.
        /// </summary>
        /// <param name="ids">Sub-fingerprint ids.</param>
        /// <param name="mediaType">Media type.</param>
        /// <returns>List of sub fingerprints.</returns>
        IEnumerable<SubFingerprintData> ReadSubFingerprintsByUid(IEnumerable<uint> ids, MediaType mediaType);

        /// <summary>
        ///  Reads Audio/Video hashes by track ID.
        /// </summary>
        /// <param name="trackId">Track ID.</param>
        /// <returns>An instance of the <see cref="AVHashes"/> class.</returns>
        AVHashes ReadAvHashesByTrackId(string trackId);
        
        /// <summary>
        ///  Adds spectral images for track reference.
        /// </summary>
        /// <param name="trackReference">Track associated with spectral images.</param>
        /// <param name="images">Spectral images to add.</param>
        void AddSpectralImages(IModelReference trackReference, IEnumerable<float[]> images);

        /// <summary>
        ///  Gets spectral images for track reference.
        /// </summary>
        /// <param name="trackReference">Track reference.</param>
        /// <returns>List of spectral images.</returns>
        IEnumerable<SpectralImageData> GetSpectralImagesByTrackReference(IModelReference trackReference);
    }
}