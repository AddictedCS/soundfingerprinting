namespace SoundFingerprinting.Query
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFingerprintingQueryUnit
    {
        Task<QueryResult> Query();

        Task<QueryResult> Query(CancellationToken cancelationToken);
    }
}