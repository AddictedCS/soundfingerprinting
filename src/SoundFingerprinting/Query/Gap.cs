// ReSharper disable UnusedMember.Local
namespace SoundFingerprinting.Query
{
    using System;
    using ProtoBuf;

    [ProtoContract]
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

        private Gap()
        {
            // left for proto-buf
        }

        /// <summary>
        ///  Gets start of the gap
        /// </summary>
        [ProtoMember(1)]
        public double Start { get; }

        /// <summary>
        ///  Gets end of the gap
        /// </summary>
        [ProtoMember(2)]
        public double End { get; }

        /// <summary>
        ///  Gets gaps length in seconds
        /// </summary>
        public double LengthInSeconds => End - Start;

        /// <summary>
        ///  Gets flag that shows if a discontinuity is on edge (i.e. either beginning or end)
        /// </summary>
        [ProtoMember(3)]
        public bool IsOnEdge { get; }

        public override string ToString()
        {
            return $"{Start:0.00}-{End:0.00}";
        }
    }
}
