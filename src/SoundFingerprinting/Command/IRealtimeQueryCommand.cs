namespace SoundFingerprinting.Command
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRealtimeQueryCommand
    {
        Task<double> Query(CancellationToken cancellationToken);
    }
}