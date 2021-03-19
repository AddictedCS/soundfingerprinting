namespace SoundFingerprinting.Command
{
    using System.Threading;
    using System.Threading.Tasks;
    using SoundFingerprinting.Audio;

    /// <summary>
    ///  Collection that emits realtime audio samples from the underlying source.
    /// </summary>
    public interface IRealtimeCollection
    {
        /// <summary>
        ///  Get audio samples from underlying source.
        /// </summary>
        /// <param name="cancellationToken">
        ///  Cancellation token used to cancel reading from the underlying source.
        /// </param>
        /// <returns>
        ///  Instance of <see cref="AudioSamples"/> or null when cancellation token is requested or underlying source stopped emitting audio samples.
        /// </returns>
        Task<AudioSamples?> TryReadAsync(CancellationToken cancellationToken);
    }
}