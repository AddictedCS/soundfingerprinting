namespace SoundFingerprinting.Command
{
    using System.Collections.Concurrent;
    using SoundFingerprinting.Audio;

    public interface IRealtimeSource
    {
        IWithRealtimeQueryConfiguration From(BlockingCollection<AudioSamples> audioSamples);

        IWithRealtimeQueryConfiguration From(IRealtimeCollection realtimeCollection);
    }
}