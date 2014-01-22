namespace SoundFingerprinting.Command
{
    using System.Threading.Tasks;

    using SoundFingerprinting.Query;

    public interface IQueryCommand
    {
        Task<QueryResult> Query();
    }
}