namespace SoundFingerprinting.Command
{
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query;

    public interface IQueryCommand
    {
        /// <summary>
        /// Gets fingerprint configuration used to create fingerprints during query snippet processing.
        /// </summary>
        FingerprintConfiguration FingerprintConfiguration { get; }

        /// <summary>
        /// Gets query configuration used to query the underlying data source with fingerprints created from the query snippet
        /// </summary>
        QueryConfiguration QueryConfiguration { get; }

        /// <summary>
        ///  Query the underlying data source, capturing best candidate and similarity information
        /// </summary>
        /// <returns>Query result</returns>
        Task<QueryResult> Query();
        
        /// <summary>
        ///  Query the underlying data source capturing best candidate as well as time location within the track
        /// </summary>
        /// <returns>Query result</returns>
        Task<QueryResult> QueryWithTimeSequenceInformation();
    }
}