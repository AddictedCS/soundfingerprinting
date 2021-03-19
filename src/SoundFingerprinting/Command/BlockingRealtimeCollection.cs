namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using SoundFingerprinting.Audio;

    /// <summary>
    ///  Blocking realtime collection implementation of realtime audio samples gathering.
    /// </summary>
    public class BlockingRealtimeCollection : IRealtimeCollection
    {
        private readonly TimeSpan delay =  TimeSpan.FromMilliseconds(1_000);
        private readonly BlockingCollection<AudioSamples> collection;

        /// <summary>
        ///  Creates new instance of BlockingRealtimeCollection class.
        /// </summary>
        /// <param name="collection"></param>
        public BlockingRealtimeCollection(BlockingCollection<AudioSamples> collection)
        {
            this.collection = collection;
        }

        /// <inheritdoc cref="IRealtimeCollection.TryReadAsync"/>
        public async Task<AudioSamples?> TryReadAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && !(collection.IsAddingCompleted && collection.Count == 0))
            {
                try
                {
                    if (collection.TryTake(out var samples, (int) delay.TotalMilliseconds, cancellationToken))
                    {
                        return await Task.FromResult(samples);
                    }
                }
                catch (Exception e) when (e is ObjectDisposedException || e is TaskCanceledException)
                {
                    return null;
                }
            }

            return null;
        }
    }
}