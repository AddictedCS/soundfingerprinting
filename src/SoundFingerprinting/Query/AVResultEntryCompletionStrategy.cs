namespace SoundFingerprinting.Query
{
    using System;
    using SoundFingerprinting.Configuration;

    internal sealed class AvResultEntryCompletionStrategy : ICompletionStrategy<AVResultEntry>
    {
        private readonly ICompletionStrategy<ResultEntry> audioStrategy;
        private readonly ICompletionStrategy<ResultEntry> videoStrategy;

        public AvResultEntryCompletionStrategy(ICompletionStrategy<ResultEntry> audioStrategy, ICompletionStrategy<ResultEntry> videoStrategy)
        {
            this.audioStrategy = audioStrategy ?? throw new ArgumentNullException(nameof(audioStrategy));
            this.videoStrategy = videoStrategy ?? throw new ArgumentNullException(nameof(videoStrategy));
        }

        public AvResultEntryCompletionStrategy(AVQueryConfiguration config, double stalenessCeiling = double.MaxValue) : this(
            new ResultEntryCompletionStrategy(config.Audio.PermittedGap, stalenessCeiling),
            new ResultEntryCompletionStrategy(config.Video.PermittedGap, stalenessCeiling))
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