namespace SoundFingerprinting.Command
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using SoundFingerprinting.Query;

    /// <summary>
    ///  Query command.
    /// </summary>
    public interface IQueryCommand
    {
        /// <summary>
        ///  Query the underlying <see cref="IQueryService"/>.
        /// </summary>
        /// <param name="cancellationToken">Token that cancels the storage round trips and the match registration; fingerprint generation runs to completion.</param>
        /// <returns>Query result.</returns>
        Task<AVQueryResult> QueryAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///  Query the underlying <see cref="IQueryService"/>.
        /// </summary>
        /// <param name="relativeTo">The timestamp which is considered as a reference point of the query operation. Will be used to set exact <see cref="ResultEntry.MatchedAt"/> timestamp of the match.</param>
        /// <param name="cancellationToken">Token that cancels the storage round trips and the match registration; fingerprint generation runs to completion.</param>
        /// <returns>Query result.</returns>
        Task<AVQueryResult> QueryAsync(DateTime relativeTo, CancellationToken cancellationToken = default);
    }
}