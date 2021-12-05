namespace SoundFingerprinting.Command
{
    using System;
    using System.Threading.Tasks;
    using SoundFingerprinting.Query;

    /// <summary>
    ///  Class that holds <see cref="AVQueryResult"/> from a previous query.
    /// </summary>
    public class ExecutedQueryCommand : IQueryCommand
    {
        private readonly AVQueryResult queryResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutedQueryCommand"/> class.
        /// </summary>
        /// <param name="queryResult">Query result.</param>
        public ExecutedQueryCommand(AVQueryResult queryResult)
        {
            this.queryResult = queryResult;
        }

        /// <summary>
        ///  Return results from a previously executed command.
        /// </summary>
        /// <returns>Audio/Video query result.</returns>
        public Task<AVQueryResult> Query()
        {
            return Task.FromResult(queryResult);
        }

        /// <summary>
        ///  Return results from a previously executed command.
        /// </summary>
        /// <param name="relativeTo">Relative to (ignored on executed query command).</param>
        /// <returns>Audio/Video query result.</returns>
        public Task<AVQueryResult> Query(DateTime relativeTo)
        {
            return Task.FromResult(queryResult);
        }
    }
}