namespace SoundFingerprinting.Command
{
    using System.Threading.Tasks;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query;

    public interface IQueryCommand
    {
        FingerprintConfiguration FingerprintConfiguration { get; }

        QueryConfiguration QueryConfiguration { get; }

        Task<QueryResult> Query();
        
        Task<QueryResult> Query2();

        Task<QueryResult> Query3();
    }
}