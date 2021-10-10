namespace SoundFingerprinting.Audio
{
    /// <summary>
    ///  Realtime samples aggregator.
    /// </summary>
    public interface IRealtimeAudioSamplesAggregator
    {
        /// <summary>
        /// Aggregates audio samples chunks before issuing a query request.
        /// </summary>
        /// <param name="chunk">Chunk to aggregate.</param>
        /// <returns>If minimum size is reached, instance of <see cref="AudioSamples"/> is returned for the query.</returns>
        AudioSamples? Aggregate(AudioSamples chunk);
    }
}