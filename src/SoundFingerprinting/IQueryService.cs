namespace SoundFingerprinting;

using System.Collections.Generic;
using SoundFingerprinting.Configuration;
using SoundFingerprinting.DAO;
using SoundFingerprinting.DAO.Data;
using SoundFingerprinting.Data;
using SoundFingerprinting.LCS;

/// <summary>
///  Query service interface.
/// </summary>
public interface IQueryService
{
    /// <summary>
    ///  Queries the underlying storage with hashes and query configuration.
    /// </summary>
    /// <param name="hashes">Query hashes.</param>
    /// <param name="config">Query configuration.</param>
    /// <returns>An instance of <see cref="Candidates"/>.</returns>
    Candidates QueryEfficiently(Hashes hashes, QueryConfiguration config);
    
    /// <summary>
    ///  Read tracks by model references.
    /// </summary>
    /// <param name="references">List of model references to read.</param>
    /// <returns>List of tracks.</returns>
    IEnumerable<TrackData> ReadTracksByReferences(IEnumerable<IModelReference> references);
}