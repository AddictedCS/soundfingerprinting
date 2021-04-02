namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using SoundFingerprinting.Audio;

    public interface IRealtimeSource
    {
        IWithRealtimeQueryConfiguration From(IAsyncEnumerable<AudioSamples> realtimeCollection);
    }
}