namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;

    /// <summary>
    /// Marker class which sorts internally stored sub-fingerprint entries by their audio position representation
    /// E.g. entries will be sorted as [1.48, 2.96, ...]
    /// </summary>
    internal class SubfingerprintSetSortedByTimePosition : SortedSet<SubFingerprintData>
    {
        public SubfingerprintSetSortedByTimePosition() : base(new SubFingerprintSequenceComparer())
        {
        }
    }
}
