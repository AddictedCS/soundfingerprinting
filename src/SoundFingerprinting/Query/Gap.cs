using System;

namespace SoundFingerprinting.Query
{
    public class Gap
    {
        public Gap(double start, double end, bool isOnEdge)
        {
            if (end <= start)
            {
                throw new ArgumentException($"{nameof(start)}={start} must be less than {nameof(end)}={end}");
            }

            Start = start;
            End = end;
            IsOnEdge = isOnEdge;
        }

        public double Start { get; }

        public double End { get; }

        public double LengthInSeconds => End - Start;

        /// <summary>
        ///  Gets flag that shows if a discontinuity is on edge (i.e. either beginning or end)
        /// </summary>
        public bool IsOnEdge { get; }

        public override string ToString()
        {
            return $"{Start:0.00}-{End:0.00}";
        }
    }
}
