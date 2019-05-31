namespace SoundFingerprinting
{
    using System.Collections.Generic;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    public interface IQueryMatchRegistry
    {
        /// <summary>
        ///  Register successful matches
        /// </summary>
        /// <param name="queryMatches">Query matches to register as successful matches</param>
        void RegisterMatches(IEnumerable<QueryMatch> queryMatches);

        /// <summary>
        ///  Register query matches as result entries
        /// </summary>
        /// <param name="resultEntries">Result entries to register as query matches</param>
        void RegisterMatches(IEnumerable<ResultEntry> resultEntries);
    }
}