namespace SoundFingerprinting.Builder
{
    using SoundFingerprinting.Command;

    public interface IQueryCommandBuilder
    {
        /// <summary>
        ///  Start building query command for audio identification
        /// </summary>
        /// <returns>Source selector</returns>
        IQuerySource BuildQueryCommand();
    }
}