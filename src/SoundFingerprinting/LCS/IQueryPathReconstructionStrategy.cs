﻿namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.Query;

    internal interface IQueryPathReconstructionStrategy
    {
        /// <summary>
        ///  Get reconstructed best paths from the list of matched entries.
        /// </summary>
        /// <param name="matches">Matches returned by the query.</param>
        /// <param name="permittedGap">Permitted gap.</param>
        /// <returns>
        ///   List of query paths. <br />
        ///   Best paths will always return in the decreasing order or their length.
        /// </returns>
        IEnumerable<IEnumerable<MatchedWith>> GetBestPaths(IEnumerable<MatchedWith> matches, double permittedGap);
    }
}