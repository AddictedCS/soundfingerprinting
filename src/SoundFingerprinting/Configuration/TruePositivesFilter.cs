namespace SoundFingerprinting.Configuration;

using System;
using SoundFingerprinting.LCS;

/// <inheritdoc cref="ITruePositivesFilter"/>
public class TruePositivesFilter : ITruePositivesFilter
{
    private readonly double minConfidence;
    private readonly double minRelativeCoverage;
    private readonly double minCoverageLength;

    /// <summary>
    ///  Initializes a new instance of the <see cref="TruePositivesFilter"/> class.
    /// </summary>
    /// <param name="minConfidence">Min confidence value.</param>
    /// <param name="minRelativeCoverage">Min relative coverage value.</param>
    /// <param name="minCoverageLength">Min coverage length.</param>
    /// <exception cref="ArgumentException">
    ///  Thrown when any of the parameters is out of range.
    /// </exception>
    /// <remarks>
    ///  If any of the thresholds is met, the coverage is considered a true positive.
    /// </remarks>
    public TruePositivesFilter(double minConfidence, double minRelativeCoverage, double minCoverageLength)
    {
        if (minConfidence is < 0 or > 1)
        {
            throw new ArgumentException("Minimal confidence should be in range [0, 1]", nameof(minConfidence));
        }
        
        if (minRelativeCoverage is < 0 or > 1)
        {
            throw new ArgumentException("Minimal relative coverage should be in range [0, 1]", nameof(minRelativeCoverage));
        }
        
        if (minCoverageLength < 0)
        {
            throw new ArgumentException("Minimal coverage length should be non-negative", nameof(minCoverageLength));
        }
        
        this.minConfidence = minConfidence;
        this.minRelativeCoverage = minRelativeCoverage;
        this.minCoverageLength = minCoverageLength;
    }

    /// <inheritdoc cref="ITruePositivesFilter.IsTruePositive"/>
    public bool IsTruePositive(Coverage coverage)
    {
        if (coverage.Confidence >= minConfidence)
        {
            return true;
        }
        
        if (coverage.TrackRelativeCoverage >= minRelativeCoverage || coverage.QueryRelativeCoverage >= minRelativeCoverage)
        {
            return true;
        }
        
        if (coverage.TrackCoverageWithPermittedGapsLength >= minCoverageLength || coverage.QueryCoverageWithPermittedGapsLength >= minCoverageLength)
        {
            return true;
        }

        return false;
    }
}