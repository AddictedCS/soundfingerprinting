namespace SoundFingerprinting.LCS
{
    using System.Collections.Generic;

    using SoundFingerprinting.DAO.Data;

    public class SubFingerprintSequenceComparer : IComparer<SubFingerprintData>
    {
        public int Compare(SubFingerprintData x, SubFingerprintData y)
        {
            return x.SequenceAt.CompareTo(y.SequenceAt);
        }
    }
}