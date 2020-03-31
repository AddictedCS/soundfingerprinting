namespace SoundFingerprinting
{
    using System.Collections.Generic;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;

    public interface IModelService
    {
        /// <summary>
        ///  Gets details about underlying storage (i.e. track count, hashes count etc.)
        /// </summary>
        IEnumerable<ModelServiceInfo> Info { get; }

        /// <summary>
        ///  Inserts new track with associated hashes into the storage
        /// </summary>
        /// <param name="trackInfo">Track information</param>
        /// <param name="hashes">Associated hashes computed by SoundFingerprinting algorithm</param>
        void Insert(TrackInfo trackInfo, Hashes hashes);

        /// <summary>
        ///  Queries the underlying storage with hashes and query configuration
        /// </summary>
        /// <param name="hashes">Computed hashes for query</param>
        /// <param name="config">Query configuration</param>
        /// <returns>List of matched fingerprints</returns>
        IEnumerable<SubFingerprintData> Query(IEnumerable<int[]> hashes, QueryConfiguration config);
        
        /// <summary>
        ///  Read tracks by model references
        /// </summary>
        /// <param name="references">List of model references to read</param>
        /// <returns>List of tracks</returns>
        IEnumerable<TrackData> ReadTracksByReferences(IEnumerable<IModelReference> references);
        
        /// <summary>
        ///  Removes a track and associated hashes from the storage by track ID
        /// </summary>
        /// <param name="trackId"></param>
        /// <returns></returns>
        int DeleteTrack(string trackId);

        /// <summary>
        ///  Read track by ID
        /// </summary>
        /// <param name="trackId">Track ID to read the track</param>
        /// <returns>TrackInfo if storage contains a track with provided ID, null otherwise</returns>
        TrackInfo? ReadTrackById(string trackId);
        
        /// <summary>
        ///  Read all tracks from the storage
        /// </summary>
        /// <returns>List of all inserted tracks</returns>
        IEnumerable<TrackData> ReadAllTracks();
    }
}
