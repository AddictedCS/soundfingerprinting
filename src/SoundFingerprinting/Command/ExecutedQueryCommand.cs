namespace SoundFingerprinting.Command
{
    using System;
    using System.Threading.Tasks;
    using SoundFingerprinting.Query;

    public class ExecutedQueryCommand : IQueryCommand
    {
        private readonly QueryResult queryResult;

        public ExecutedQueryCommand(QueryResult queryResult)
        {
            this.queryResult = queryResult;
        }
        
        public Task<QueryResult> Query()
        {
            return Task.FromResult(queryResult);
        }

        public Task<QueryResult> Query(DateTime relativeTo)
        {
            return Task.FromResult(queryResult);
        }
    }
}