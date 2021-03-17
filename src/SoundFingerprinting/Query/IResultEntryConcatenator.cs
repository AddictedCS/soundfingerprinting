namespace SoundFingerprinting.Query
{
    public interface IResultEntryConcatenator
    {
        ResultEntry Concat(ResultEntry? left, ResultEntry? right, double queryOffset = 0d);
    }
}