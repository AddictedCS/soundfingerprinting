namespace SoundFingerprinting.Command
{
    using System.Threading;
    using SoundFingerprinting.Audio;

    public interface IRealtimeCollection
    {
        bool TryTake(out AudioSamples audioSamples, int millisecondsDelay, CancellationToken cancellationToken);

        bool IsAddingCompleted { get; }
    }
}