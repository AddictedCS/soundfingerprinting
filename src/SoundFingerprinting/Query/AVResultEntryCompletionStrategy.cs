namespace SoundFingerprinting.Query
{
    using System;
    using SoundFingerprinting.Configuration;

    internal sealed class AVResultEntryCompletionStrategy : ICompletionStrategy<AVResultEntry>
    {
        private readonly ICompletionStrategy<ResultEntry> audioStrategy;
        private readonly ICompletionStrategy<ResultEntry> videoStrategy;

        public AVResultEntryCompletionStrategy(ICompletionStrategy<ResultEntry> audioStrategy, ICompletionStrategy<ResultEntry> videoStrategy)
        {
            this.audioStrategy = audioStrategy ?? throw new ArgumentNullException(nameof(audioStrategy));
            this.videoStrategy = videoStrategy ?? throw new ArgumentNullException(nameof(videoStrategy));
        }

        public AVResultEntryCompletionStrategy(AVQueryConfiguration config) : this(
            new ResultEntryCompletionStrategy(),
            new ResultEntryCompletionStrategy())
        {
            // no-op
        }

        /// <inheritdoc cref="ICompletionStrategy{T}.CanContinueInNextQuery"/>
        public bool CanContinueInNextQuery(AVResultEntry? entry)
        {
            if (entry == null)
            {
                return false;
            }

            return audioStrategy.CanContinueInNextQuery(entry.Audio) || videoStrategy.CanContinueInNextQuery(entry.Video);
        }
    }
}