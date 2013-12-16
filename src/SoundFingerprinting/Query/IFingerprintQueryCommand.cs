namespace SoundFingerprinting.Query
{
    using System.Threading.Tasks;

    public interface IFingerprintQueryCommand
    {
        Task<QueryResult> Query();
    }
}