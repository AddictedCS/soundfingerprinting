namespace SoundFingerprinting.Command
{
    using System;
    using System.Threading.Tasks;

    using SoundFingerprinting.Query;

    public interface IQueryCommand
    {
        /// <summary>
        ///  Query the underlying data source, capturing best candidate and similarity information
        /// </summary>
        /// <returns>Query result</returns>
        Task<QueryResult> Query();

        /// <summary>
        ///  Query the underlying data source, capturing best candidate and similarity information
        /// </summary>
        /// <param name="relativeTo">The timestamp which is considered as a reference point of the query operation. Useful when you would like to re-query the storage with previously generated fingerprints</param>
        /// <returns>Query result</returns>
        Task<QueryResult> Query(DateTime relativeTo);
    }
}