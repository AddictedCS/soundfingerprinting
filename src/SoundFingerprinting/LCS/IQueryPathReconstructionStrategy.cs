namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.Query;

    internal interface IQueryPathReconstructionStrategy
    {
        /// <summary>
        ///  Get reconstructed best paths from the list of matched entries.
        /// </summary>
        /// <param name="matches">Matches returned by the query.</param>
        /// <param name="maxGap">Maximum allowed gap.</param>
        /// <returns>List of query paths.</returns>
        IEnumerable<IEnumerable<MatchedWith>> GetBestPaths(IEnumerable<MatchedWith> matches, double maxGap);
    }
}