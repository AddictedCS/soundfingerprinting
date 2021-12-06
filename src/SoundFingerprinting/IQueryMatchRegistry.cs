namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using SoundFingerprinting.Query;

    public interface IQueryMatchRegistry
    {
        /// <summary>
        ///  Register successful matches.
        /// </summary>
        /// <param name="queryMatches">Query matches to register as successful matches.</param>
        void RegisterMatches(IEnumerable<AVQueryMatch> queryMatches);
    }
}