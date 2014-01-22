namespace SoundFingerprinting.Utils
{
    using System;
    using System.Collections.Generic;

    internal class AbsComparator : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            return Math.Abs(y).CompareTo(Math.Abs(x));
        }
    }
}
