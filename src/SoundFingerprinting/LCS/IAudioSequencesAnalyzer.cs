namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;

    internal interface IAudioSequencesAnalyzer
    {
        /// <summary>
        /// Sorts the list of sub-fingerprints taking into account the length of the increasing subsequences
        /// E.g. [{1, [2,3,4,5,6]}, {2, [3,4,6,8,70,90]}] - the first entry with track id 1, is going to be returned as the most probable candidate
        /// </summary>
        /// <param name="candidates">List of candidates returned by the model service</param>
        /// <param name="queryLength">The length of the query</param>
        /// <returns>Sorted list of fingerprints, grouped by most-probable first</returns>
        IEnumerable<IEnumerable<SubFingerprintData>> SortCandiatesByLongestIncresingAudioSequence(Dictionary<IModelReference, SubfingerprintSetSortedByTimePosition> candidates, double queryLength);
    }
}