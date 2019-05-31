namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    internal class NoOpQueryMatchRegistry : QueryMatchRegistry
    {
        public static NoOpQueryMatchRegistry NoOp = new NoOpQueryMatchRegistry();

        public override void RegisterMatches(IEnumerable<QueryMatch> queryMatches)
        {
            // no op
        }

        public override void RegisterMatches(IEnumerable<ResultEntry> resultEntries)
        {
            // no op
        }
    }
}