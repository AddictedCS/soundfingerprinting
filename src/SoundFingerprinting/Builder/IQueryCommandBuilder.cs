namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;

    public interface IQueryCommandBuilder
    {
        IQuerySource BuildQueryCommand();
    }
}