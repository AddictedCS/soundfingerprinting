namespace SoundFingerprinting.Configuration;

/// <inheritdoc cref="ITruePositivesFilter"/>
public class EmpiricalTruePositivesFilter() : TruePositivesFilter(minConfidence: 0.2, minRelativeCoverage: 0.2, minCoverageLength: 5)
{
}