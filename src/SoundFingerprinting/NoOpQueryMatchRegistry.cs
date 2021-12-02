namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using SoundFingerprinting.Query;

    internal class NoOpQueryMatchRegistry : IQueryMatchRegistry
    {
        public static readonly NoOpQueryMatchRegistry NoOp = new ();

        public void RegisterMatches(IEnumerable<AVQueryMatch> queryMatches)
        {
            // no op
        }
    }
}