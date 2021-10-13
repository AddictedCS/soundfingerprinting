namespace SoundFingerprinting.Command
{
    using System;
    using System.Threading.Tasks;

    using SoundFingerprinting.Query;

    /// <summary>
    ///  Query command.
    /// </summary>
    public interface IQueryCommand
    {
        /// <summary>
        ///  Query the underlying <see cref="IModelService"/>.
        /// </summary>
        /// <returns>Query result.</returns>
        Task<QueryResult> Query();

        /// <summary>
        ///  Query the underlying <see cref="IModelService"/>.
        /// </summary>
        /// <param name="relativeTo">The timestamp which is considered as a reference point of the query operation. Will be used to set exact <see cref="ResultEntry.MatchedAt"/> timestamp of the match.</param>
        /// <returns>Query result.</returns>
        Task<QueryResult> Query(DateTime relativeTo);
    }
}