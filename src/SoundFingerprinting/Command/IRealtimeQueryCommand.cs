namespace SoundFingerprinting.Command
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRealtimeQueryCommand
    {
        /// <summary>
        ///  Query in realtime
        /// </summary>
        /// <param name="cancellationToken">Token to cancel continuous query</param>
        /// <returns>Queried number of seconds</returns>
        Task<double> Query(CancellationToken cancellationToken);

        /// <summary>
        ///  Hashes audio samples passed through the blocking collection. Invokes fingerprints callback, but does not query the underlying source.
        ///  This method can be used to generate offline hashes that can be used later to replay (re-query certain dates).
        /// </summary>
        /// <param name="cancellationToken">Token to cancel continuous hash generation</param>
        /// <returns>Hashed number of seconds</returns>
        Task<double> Hash(CancellationToken cancellationToken);
    }
}