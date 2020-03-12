namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;

    internal class NoOpQueryMatchRegistry : IQueryMatchRegistry
    {
        public static readonly NoOpQueryMatchRegistry NoOp = new NoOpQueryMatchRegistry();

        public void RegisterMatches(IEnumerable<QueryMatch> queryMatches, IDictionary<string, string> meta)
        {
            // no op
        }
    }
}