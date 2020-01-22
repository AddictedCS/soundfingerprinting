namespace SoundFingerprinting.LCS
{
    using SoundFingerprinting.Query;

    internal class MaxAt
    {
        public MaxAt(int length, MatchedWith matchedWith)
        {
            Length = length;
            MatchedWith = matchedWith;
        }

        public int Length { get; }
        
        public MatchedWith MatchedWith { get; }
    }
}