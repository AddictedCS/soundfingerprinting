namespace SoundFingerprinting;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SoundFingerprinting.Configuration;
using SoundFingerprinting.DAO;
using SoundFingerprinting.DAO.Data;
using SoundFingerprinting.Data;
using SoundFingerprinting.LCS;

/// <summary>
///  Query service interface.
/// </summary>
/// <remarks>
///  An implementation over a local storage may compute synchronously and return a completed task. A remote implementation awaits its I/O.
/// </remarks>
public interface IQueryService
{
    /// <summary>
    ///  Queries the underlying storage with hashes and query configuration.
    /// </summary>
    /// <param name="hashes">Query hashes.</param>
    /// <param name="config">Query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An instance of <see cref="Candidates"/>.</returns>
    Task<Candidates> QueryEfficientlyAsync(Hashes hashes, QueryConfiguration config, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Read tracks by model references.
    /// </summary>
    /// <param name="references">List of model references to read.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Materialized list of tracks; enumerating it performs no I/O.</returns>
    Task<IReadOnlyList<TrackData>> ReadTracksByReferencesAsync(IEnumerable<IModelReference> references, CancellationToken cancellationToken = default);
}
