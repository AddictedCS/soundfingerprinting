namespace SoundFingerprinting.Command
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///  Blocking realtime collection implementation of realtime audio samples gathering.
    /// </summary>
    /// <typeparam name="T">Enclosing T type of the collection.</typeparam>
    public class BlockingRealtimeCollection<T> : IAsyncEnumerable<T> where T : class
    {
        private readonly TimeSpan delay;
        private readonly BlockingCollection<T> collection;

        /// <summary>
        ///  Initializes a new instance of the <see cref="BlockingRealtimeCollection{T}"/> class.
        /// </summary>
        /// <param name="collection">Instance of blocking collection to iterate over.</param>
        public BlockingRealtimeCollection(BlockingCollection<T> collection) : this(collection, TimeSpan.FromMilliseconds(30_000))
        {
            // no op
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="BlockingRealtimeCollection{T}"/> class.
        /// </summary>
        /// <param name="collection">Instance of blocking collection to iterate over.</param>
        /// <param name="delay">Delay to use between consecutive polling of the inner collection.</param>
        public BlockingRealtimeCollection(BlockingCollection<T> collection, TimeSpan delay)
        {
            this.collection = collection;
            this.delay = delay;
        }

        /// <summary>
        ///  Gets async enumerable allowing async iteration over the realtime collection.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel iteration.</param>
        /// <returns>Instance of async enumerable.</returns>
        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            T? result;
            while ((result = await TryReadAsync(cancellationToken)) != null)
            {
                yield return result;
            }
        }
        
        private async Task<T?> TryReadAsync(CancellationToken cancellationToken)
        {
            while (!collection.IsAddingCompleted || collection.Count > 0)
            {
                try
                {
                    if (collection.TryTake(out var samples, (int)delay.TotalMilliseconds, cancellationToken))
                    {
                        return await Task.FromResult(samples);
                    }
                }
                catch (Exception e) when (e is ObjectDisposedException or OperationCanceledException)
                {
                    return null;
                }
            }

            return null;
        }
    }
}