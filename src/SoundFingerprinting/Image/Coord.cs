namespace SoundFingerprinting.Image
{
    public struct Coord
    {
        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }

        public override string ToString()
        {
            return $"(X={X},Y={Y})";
        }
    }
}