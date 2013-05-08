namespace Soundfingerprinting.Query
{
    using System.Threading.Tasks;

    public interface IFingerprintingQueryUnit
    {
        Task<QueryResult> Query();
    }
}