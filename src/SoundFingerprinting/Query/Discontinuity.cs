using System;

namespace SoundFingerprinting.Query
{
    public class Discontinuity
    {
        public Discontinuity(float start, float end)
        {
            if (end <= start)
                throw new ArgumentException($"{nameof(start)}={start} must be less than {nameof(end)}={end}");

            Start = start;
            End = end;
        }

        public float Start { get; }

        public float End { get; }

        public float LengthInSeconds
        {
            get
            {
                return End - Start;
            }
        }

        public override string ToString()
        {
            return $"{Start:0.00}-{End:0.00}";
        }
    }
}
