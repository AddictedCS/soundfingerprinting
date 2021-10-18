namespace SoundFingerprinting.Command
{
    using System;

    /// <summary>
    ///  Backoff policy 
    /// </summary>
    public interface IBackoffPolicy
    {
        /// <summary>
        ///  Notify success.
        /// </summary>
        void Success();

        /// <summary>
        ///  Notify failure.
        /// </summary>
        void Failure();

        /// <summary>
        ///  Gets a flag indicating whether it can retry.
        /// </summary>
        bool CanRetry { get; }

        /// <summary>
        ///  Gets an instance of the <see cref="TimeSpan"/> class indicating remaining delay.
        /// </summary>
        TimeSpan RemainingDelay { get; }
    }
}