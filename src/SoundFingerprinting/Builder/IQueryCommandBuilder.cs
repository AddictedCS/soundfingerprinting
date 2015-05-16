namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;

    public interface IQueryCommandBuilder
    {
        /// <summary>
        /// Start building the query command
        /// </summary>
        /// <returns>Source selector</returns>
        IQuerySource BuildQueryCommand();
    }
}