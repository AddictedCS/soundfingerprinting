namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.LCS;

    public interface IConcatenator<T> where T : class
    {
        /// <summary>
        ///  Concatenate incoming result entries from a streaming source.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <param name="queryOffset">Query offset (if any).</param>
        /// <returns>New instance of type T.</returns>
        T Concat(T? left, T? right, double queryOffset = 0d);

        /// <summary>
        ///  Extends query length on coverage object.
        /// </summary>
        /// <param name="entry">Entry to extend coverage on.</param>
        /// <param name="length">Length to add to <see cref="Coverage.QueryLength"/> in the resulting object.</param>
        /// <returns>New instance of T with updated query length.</returns>
        T WithExtendedQueryLength(T entry, double length);
    }
}