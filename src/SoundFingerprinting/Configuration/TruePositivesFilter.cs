namespace SoundFingerprinting.Configuration;

using System;
using SoundFingerprinting.LCS;

/// <inheritdoc cref="ITruePositivesFilter"/>
public class TruePositivesFilter : ITruePositivesFilter
{
    private readonly double? minConfidence;
    private readonly double? minRelativeCoverage;
    private readonly double? minCoverageLength;
    private readonly FilterLogic logic;

    /// <summary>
    ///  Initializes a new instance of the <see cref="TruePositivesFilter"/> class.
    /// </summary>
    /// <param name="minConfidence">Min confidence value.</param>
    /// <param name="minRelativeCoverage">Min relative coverage value.</param>
    /// <param name="minCoverageLength">Min coverage length.</param>
    /// <param name="logic">Logic used to combine conditions. Defaults to <see cref="FilterLogic.Or"/>.</param>
    /// <exception cref="ArgumentException">
    ///  Thrown when any of the parameters is out of range.
    /// </exception>
    /// <remarks>
    ///  When using <see cref="FilterLogic.Or"/>, a coverage is considered a true positive if any of the non-null thresholds is met.
    ///  When using <see cref="FilterLogic.And"/>, all non-null thresholds must be met for a coverage to be considered a true positive.
    /// </remarks>
    public TruePositivesFilter(double? minConfidence, double? minRelativeCoverage, double? minCoverageLength, FilterLogic logic = FilterLogic.Or)
    {
        if (minConfidence is null && minRelativeCoverage is null && minCoverageLength is null)
        {
            throw new ArgumentException("At least one of the parameters should be non-null");
        }
        
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
        this.logic = logic;
    }

    /// <inheritdoc cref="ITruePositivesFilter.IsTruePositive"/>
    public bool IsTruePositive(Coverage coverage)
    {
        return logic == FilterLogic.Or ? IsTruePositiveOr(coverage) : IsTruePositiveAnd(coverage);
    }
    
    private bool IsTruePositiveOr(Coverage coverage)
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
    
    private bool IsTruePositiveAnd(Coverage coverage)
    {
        if (minConfidence.HasValue && coverage.Confidence < minConfidence)
        {
            return false;
        }
        
        if (minRelativeCoverage.HasValue && coverage.TrackRelativeCoverage < minRelativeCoverage && coverage.QueryRelativeCoverage < minRelativeCoverage)
        {
            return false;
        }
        
        if (minCoverageLength.HasValue && coverage.TrackCoverageWithPermittedGapsLength < minCoverageLength && coverage.QueryCoverageWithPermittedGapsLength < minCoverageLength)
        {
            return false;
        }

        return true;
    }
}