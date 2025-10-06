namespace SoundFingerprinting.Configuration;

using SoundFingerprinting.LCS;
using SoundFingerprinting.Query;

/// <summary>
///  Filter that can be used to filter only true positive results.
/// </summary>
public interface ITruePositivesFilter
{
    /// <summary>
    ///  Checks if the coverage of a <see cref="ResultEntry"/> indicates a true positive.
    /// </summary>
    /// <param name="coverage">An instance of <see cref="Coverage"/> object.</param>
    /// <returns>True is positive, otherwise false.</returns>
    bool IsTruePositive(Coverage coverage);
}