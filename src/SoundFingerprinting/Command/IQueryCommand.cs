namespace SoundFingerprinting.Command
{
    using System.Threading.Tasks;

    using SoundFingerprinting.Query;

    public interface IQueryCommand
    {
        /// <summary>
        ///  Query the underlying data source, capturing best candidate and similarity information
        /// </summary>
        /// <returns>Query result</returns>
        Task<QueryResult> Query();
    }
}