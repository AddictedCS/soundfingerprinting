namespace SoundFingerprinting.Configuration;

using SoundFingerprinting.LCS;

/// <summary>
///  All matches are considered true positives.
/// </summary>
public class AllMatchesAreTruePositives : ITruePositivesFilter
{
    /// <inheritdoc cref="ITruePositivesFilter.IsTruePositive"/>
    /// <returns>True for all coverages.</returns>
    public bool IsTruePositive(Coverage coverage)
    {
        return true;
    }
}