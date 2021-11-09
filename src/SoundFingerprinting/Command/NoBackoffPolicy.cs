namespace SoundFingerprinting.Command
{
    using System;

    public class NoBackoffPolicy : IBackoffPolicy
    {
        public void Success()
        {
            // no op
        }

        public void Failure()
        {
            // no op
        }

        public bool CanRetry => false;

        public TimeSpan RemainingDelay => TimeSpan.Zero;
    }
}