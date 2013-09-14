namespace SoundFingerprinting.Utils
{
    using System;
    using System.Collections.Generic;

    public class AbsComparator : IComparer<float>
    {
        #region IComparer<float> Members
     
        public int Compare(float x, float y)
        {
            return Math.Abs(y).CompareTo(Math.Abs(x));
        }

        #endregion
    }
}