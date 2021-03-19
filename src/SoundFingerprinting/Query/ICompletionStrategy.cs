namespace SoundFingerprinting.Query
{
    /// <summary>
    ///  Completion strategy used to decide whether the result entry has completed or not.
    /// </summary>
    /// <typeparam name="T">Type that contains information about the <see cref="ResultEntry"/>.</typeparam>
    public interface ICompletionStrategy<in T> where T : class
    {
        /// <summary>
        ///  Decides whether the match can continue in the next query or not.
        /// </summary>
        /// <param name="entry">
        ///  Entry to decide upon.
        /// </param>
        /// <returns>
        ///  True if we need to wait for the next query match, otherwise false.
        /// </returns>
        bool CanContinueInNextQuery(T? entry);
    }
}