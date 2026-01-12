namespace SoundFingerprinting.Tests.Unit.Configuration;

using System;
using System.Collections.Generic;
using NUnit.Framework;
using SoundFingerprinting.Configuration;
using SoundFingerprinting.LCS;
using SoundFingerprinting.Query;

[TestFixture]
public class TruePositivesFilterTest
{
    [Test]
    public void ShouldThrowWhenAllParametersAreNull()
    {
        Assert.Throws<ArgumentException>(() => new TruePositivesFilter(null, null, null));
    }

    [Test]
    public void ShouldThrowWhenConfidenceIsNegative()
    {
        Assert.Throws<ArgumentException>(() => new TruePositivesFilter(-0.1, null, null));
    }

    [Test]
    public void ShouldThrowWhenConfidenceIsGreaterThanOne()
    {
        Assert.Throws<ArgumentException>(() => new TruePositivesFilter(1.1, null, null));
    }

    [Test]
    public void ShouldThrowWhenRelativeCoverageIsNegative()
    {
        Assert.Throws<ArgumentException>(() => new TruePositivesFilter(null, -0.1, null));
    }

    [Test]
    public void ShouldThrowWhenRelativeCoverageIsGreaterThanOne()
    {
        Assert.Throws<ArgumentException>(() => new TruePositivesFilter(null, 1.1, null));
    }

    [Test]
    public void ShouldThrowWhenCoverageLengthIsNegative()
    {
        Assert.Throws<ArgumentException>(() => new TruePositivesFilter(null, null, -1.0));
    }

    [Test]
    public void ShouldAcceptValidParameters()
    {
        Assert.That(() => new TruePositivesFilter(0.5, null, null), Throws.Nothing);
        Assert.That(() => new TruePositivesFilter(null, 0.5, null), Throws.Nothing);
        Assert.That(() => new TruePositivesFilter(null, null, 10.0), Throws.Nothing);
        Assert.That(() => new TruePositivesFilter(0.5, 0.5, 10.0), Throws.Nothing);
    }

    #region OR Logic Tests - Single Parameter (Relative Coverage)

    [Test]
    public void ShouldReturnTrueWhenRelativeCoverageMeetsThreshold_OrLogic()
    {
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.5, null, FilterLogic.Or);
        // Coverage: 60 out of 100 = 60% relative coverage
        var coverage = CreateCoverageWithRelativeCoverage(0.6);

        Assert.That(filter.IsTruePositive(coverage), Is.True);
    }

    [Test]
    public void ShouldReturnFalseWhenRelativeCoverageDoesNotMeetThreshold_OrLogic()
    {
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.5, null, FilterLogic.Or);
        // Coverage: 40 out of 100 = 40% relative coverage
        var coverage = CreateCoverageWithRelativeCoverage(0.4);

        Assert.That(filter.IsTruePositive(coverage), Is.False);
    }

    #endregion

    #region OR Logic Tests - Single Parameter (Coverage Length)

    [Test]
    public void ShouldReturnTrueWhenCoverageLengthMeetsThreshold_OrLogic()
    {
        var filter = new TruePositivesFilter(null, null, minCoverageLength: 10.0, FilterLogic.Or);
        // Coverage length of 15 seconds
        var coverage = CreateCoverageWithLength(15.0);

        Assert.That(filter.IsTruePositive(coverage), Is.True);
    }

    [Test]
    public void ShouldReturnFalseWhenCoverageLengthDoesNotMeetThreshold_OrLogic()
    {
        var filter = new TruePositivesFilter(null, null, minCoverageLength: 10.0, FilterLogic.Or);
        // Coverage length of 5 seconds
        var coverage = CreateCoverageWithLength(5.0);

        Assert.That(filter.IsTruePositive(coverage), Is.False);
    }

    #endregion

    #region OR Logic Tests - Two Parameters

    [Test]
    public void ShouldReturnTrueWhenOnlyFirstOfTwoConditionsMet_OrLogic()
    {
        // Relative coverage threshold met, but coverage length not met
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.5, minCoverageLength: 100.0, FilterLogic.Or);
        // 60% relative coverage, but only 60 seconds length (less than 100)
        var coverage = CreateCoverageWithRelativeCoverage(0.6);

        Assert.That(filter.IsTruePositive(coverage), Is.True);
    }

    [Test]
    public void ShouldReturnTrueWhenOnlySecondOfTwoConditionsMet_OrLogic()
    {
        // Relative coverage threshold not met, but coverage length met
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.9, minCoverageLength: 10.0, FilterLogic.Or);
        // 30% relative coverage, but 30 seconds length (more than 10)
        var coverage = CreateCoverageWithRelativeCoverage(0.3);

        Assert.That(filter.IsTruePositive(coverage), Is.True);
    }

    [Test]
    public void ShouldReturnTrueWhenBothConditionsMet_OrLogic()
    {
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.5, minCoverageLength: 10.0, FilterLogic.Or);
        // 60% relative coverage, 60 seconds length
        var coverage = CreateCoverageWithRelativeCoverage(0.6);

        Assert.That(filter.IsTruePositive(coverage), Is.True);
    }

    [Test]
    public void ShouldReturnFalseWhenNeitherConditionMet_OrLogic()
    {
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.9, minCoverageLength: 100.0, FilterLogic.Or);
        // 30% relative coverage, 30 seconds length
        var coverage = CreateCoverageWithRelativeCoverage(0.3);

        Assert.That(filter.IsTruePositive(coverage), Is.False);
    }

    #endregion

    #region AND Logic Tests - Single Parameter

    [Test]
    public void ShouldReturnTrueWhenRelativeCoverageMeetsThreshold_AndLogic()
    {
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.5, null, FilterLogic.And);
        var coverage = CreateCoverageWithRelativeCoverage(0.6);

        Assert.That(filter.IsTruePositive(coverage), Is.True);
    }

    [Test]
    public void ShouldReturnFalseWhenRelativeCoverageDoesNotMeetThreshold_AndLogic()
    {
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.5, null, FilterLogic.And);
        var coverage = CreateCoverageWithRelativeCoverage(0.4);

        Assert.That(filter.IsTruePositive(coverage), Is.False);
    }

    #endregion

    #region AND Logic Tests - Two Parameters

    [Test]
    public void ShouldReturnFalseWhenOnlyFirstOfTwoConditionsMet_AndLogic()
    {
        // Relative coverage threshold met, but coverage length not met
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.5, minCoverageLength: 100.0, FilterLogic.And);
        // 60% relative coverage (met), but 60 seconds length (not met, need 100)
        var coverage = CreateCoverageWithRelativeCoverage(0.6);

        Assert.That(filter.IsTruePositive(coverage), Is.False);
    }

    [Test]
    public void ShouldReturnFalseWhenOnlySecondOfTwoConditionsMet_AndLogic()
    {
        // Relative coverage threshold not met, but coverage length met
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.9, minCoverageLength: 10.0, FilterLogic.And);
        // 30% relative coverage (not met), 30 seconds length (met)
        var coverage = CreateCoverageWithRelativeCoverage(0.3);

        Assert.That(filter.IsTruePositive(coverage), Is.False);
    }

    [Test]
    public void ShouldReturnTrueWhenBothConditionsMet_AndLogic()
    {
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.5, minCoverageLength: 10.0, FilterLogic.And);
        // 60% relative coverage (met), 60 seconds length (met)
        var coverage = CreateCoverageWithRelativeCoverage(0.6);

        Assert.That(filter.IsTruePositive(coverage), Is.True);
    }

    [Test]
    public void ShouldReturnFalseWhenNeitherConditionMet_AndLogic()
    {
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.9, minCoverageLength: 100.0, FilterLogic.And);
        // 30% relative coverage (not met), 30 seconds length (not met)
        var coverage = CreateCoverageWithRelativeCoverage(0.3);

        Assert.That(filter.IsTruePositive(coverage), Is.False);
    }

    #endregion

    #region Default Logic Tests

    [Test]
    public void ShouldDefaultToOrLogic()
    {
        // Without specifying logic, should use OR (backward compatibility)
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.5, minCoverageLength: 100.0);
        // 60% relative coverage (met), 60 seconds length (not met for 100 threshold)
        var coverage = CreateCoverageWithRelativeCoverage(0.6);

        // With OR logic: relative coverage is met, so should pass
        Assert.That(filter.IsTruePositive(coverage), Is.True);
    }

    [Test]
    public void ShouldDefaultToOrLogicExplicitTest()
    {
        // Explicitly compare default behavior with explicit OR
        var filterDefault = new TruePositivesFilter(null, minRelativeCoverage: 0.5, minCoverageLength: 100.0);
        var filterExplicitOr = new TruePositivesFilter(null, minRelativeCoverage: 0.5, minCoverageLength: 100.0, FilterLogic.Or);
        var coverage = CreateCoverageWithRelativeCoverage(0.6);

        Assert.Multiple(() =>
        {
            Assert.That(filterDefault.IsTruePositive(coverage), Is.EqualTo(filterExplicitOr.IsTruePositive(coverage)));
            Assert.That(filterDefault.IsTruePositive(coverage), Is.True);
        });
    }

    [Test]
    public void ShouldDifferFromAndLogicWhenUsingDefault()
    {
        // Verify that default (OR) behaves differently from AND in the expected scenario
        var filterDefault = new TruePositivesFilter(null, minRelativeCoverage: 0.5, minCoverageLength: 100.0);
        var filterAnd = new TruePositivesFilter(null, minRelativeCoverage: 0.5, minCoverageLength: 100.0, FilterLogic.And);
        // 60% relative coverage (met), 60 seconds length (not met for 100 threshold)
        var coverage = CreateCoverageWithRelativeCoverage(0.6);

        Assert.Multiple(() =>
        {
            Assert.That(filterDefault.IsTruePositive(coverage), Is.True, "Default (OR) should pass when one condition is met");
            Assert.That(filterAnd.IsTruePositive(coverage), Is.False, "AND logic should fail when not all conditions are met");
        });
    }

    #endregion

    #region Edge Cases

    [Test]
    public void ShouldHandleExactThresholdValues_OrLogic()
    {
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.5, null, FilterLogic.Or);
        var coverageExact = CreateCoverageWithRelativeCoverage(0.5);
        // Use a significantly lower value to ensure it's below threshold after discretization
        var coverageBelow = CreateCoverageWithRelativeCoverage(0.45);

        Assert.Multiple(() =>
        {
            Assert.That(filter.IsTruePositive(coverageExact), Is.True);
            Assert.That(filter.IsTruePositive(coverageBelow), Is.False);
        });
    }

    [Test]
    public void ShouldHandleExactThresholdValues_AndLogic()
    {
        var filter = new TruePositivesFilter(null, minRelativeCoverage: 0.5, minCoverageLength: 50.0, FilterLogic.And);
        var coverageExact = CreateCoverageWithRelativeCoverage(0.5);
        // Use a significantly lower value to ensure it's below threshold after discretization
        var coverageOneBelowThreshold = CreateCoverageWithRelativeCoverage(0.45);

        Assert.Multiple(() =>
        {
            Assert.That(filter.IsTruePositive(coverageExact), Is.True, "Both conditions should be met at 50%/50 seconds");
            Assert.That(filter.IsTruePositive(coverageOneBelowThreshold), Is.False, "Relative coverage below threshold should fail AND logic");
        });
    }

    [Test]
    public void ShouldHandleSmallCoverageLength()
    {
        var filter = new TruePositivesFilter(null, null, minCoverageLength: 1.0, FilterLogic.Or);
        var coverage = CreateCoverageWithLength(2.0);

        Assert.That(filter.IsTruePositive(coverage), Is.True);
    }

    #endregion

    #region Confidence Tests
    
    [Test]
    public void ShouldReturnTrueWhenConfidenceMeetsThreshold_OrLogic()
    {
        // Use a high relative coverage which should result in high confidence
        var filter = new TruePositivesFilter(minConfidence: 0.5, null, null, FilterLogic.Or);
        var coverage = CreateCoverageWithRelativeCoverage(0.9);

        // High relative coverage usually results in high confidence
        Assert.That(filter.IsTruePositive(coverage), Is.True);
    }
    
    [Test]
    public void ShouldReturnFalseWhenConfidenceDoesNotMeetThreshold_OrLogic()
    {
        // Use a very low relative coverage which should result in low confidence
        var filter = new TruePositivesFilter(minConfidence: 0.99, null, null, FilterLogic.Or);
        var coverage = CreateCoverageWithRelativeCoverage(0.1);

        // Very low relative coverage should result in low confidence
        Assert.That(filter.IsTruePositive(coverage), Is.False);
    }
    
    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a Coverage object with the specified relative coverage.
    /// Uses a fixed track length of 100 seconds for predictable calculations.
    /// </summary>
    /// <param name="relativeCoverage">The desired relative coverage (0.0 to 1.0).</param>
    /// <returns>A Coverage object with the specified properties.</returns>
    private static Coverage CreateCoverageWithRelativeCoverage(double relativeCoverage)
    {
        const double trackLength = 100.0;
        double coverageLength = trackLength * relativeCoverage;
        return TestUtilities.GetCoverage(coverageLength, queryLength: trackLength, trackLength: trackLength, new List<Gap>());
    }

    /// <summary>
    /// Creates a Coverage object with the specified coverage length.
    /// Uses a fixed track length of 100 seconds.
    /// </summary>
    /// <param name="coverageLength">The desired coverage length in seconds.</param>
    /// <returns>A Coverage object with the specified properties.</returns>
    private static Coverage CreateCoverageWithLength(double coverageLength)
    {
        const double trackLength = 100.0;
        return TestUtilities.GetCoverage(coverageLength, queryLength: trackLength, trackLength: trackLength, new List<Gap>());
    }

    #endregion
}
