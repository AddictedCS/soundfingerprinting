namespace SoundFingerprinting.Query
{
    public class Discontinuity
    {
        public Discontinuity(float start, float end)
        {
            Start = start;
            End = end;
        }

        public float Start { get; }

        public float End { get; }

        public float Length
        {
            get
            {
                return Start - End;
            }
        }

        public override string ToString()
        {
            return $"{Start:0.00}-{End:0.00}";
        }
    }
}
