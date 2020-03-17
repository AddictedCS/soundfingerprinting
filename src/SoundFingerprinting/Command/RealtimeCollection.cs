namespace SoundFingerprinting.Command
{
    using System.Collections.Concurrent;
    using System.Threading;
    using SoundFingerprinting.Audio;

    public class RealtimeCollection : IRealtimeCollection
    {
        private readonly BlockingCollection<AudioSamples> collection;

        public RealtimeCollection(BlockingCollection<AudioSamples> collection)
        {
            this.collection = collection;
        }
        
        public bool TryTake(out AudioSamples audioSamples, int millisecondsDelay, CancellationToken cancellationToken)
        {
            return collection.TryTake(out audioSamples, millisecondsDelay, cancellationToken);
        }
        
        public bool IsFinished => collection.IsAddingCompleted && collection.Count == 0;
    }
}