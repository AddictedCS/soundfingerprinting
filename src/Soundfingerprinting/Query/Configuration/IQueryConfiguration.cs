namespace Soundfingerprinting.Query.Configuration
{
    public interface IQueryConfiguration
    {
        int NumberOfLSHTables { get; }

        int NumberOfMinHashesPerTable { get; }

        int ThresholdVotes { get; }
    }
}