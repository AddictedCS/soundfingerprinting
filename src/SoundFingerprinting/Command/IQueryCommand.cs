namespace SoundFingerprinting.Command
{
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query;

    public interface IQueryCommand
    {
        IFingerprintConfiguration FingerprintConfiguration { get; }

        IQueryConfiguration QueryConfiguration { get; }

        Task<QueryResult> Query();
    }
}