namespace SoundFingerprinting
{
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Query;

    public interface IQueryCommandBuilder
    {
        IQuerySource BuildQueryCommand();
    }
}