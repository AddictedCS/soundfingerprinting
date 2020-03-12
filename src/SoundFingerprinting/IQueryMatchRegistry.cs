namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;

    public interface IQueryMatchRegistry
    {
        /// <summary>
        ///  Register successful matches
        /// </summary>
        /// <param name="queryMatches">Query matches to register as successful matches</param>
        /// <param name="meta">Metadata related to query match</param>
        void RegisterMatches(IEnumerable<QueryMatch> queryMatches, IDictionary<string, string> meta);
    }
}