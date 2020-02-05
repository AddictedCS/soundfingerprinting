namespace SoundFingerprinting.Tests.Unit.Query
{
    using NUnit.Framework;

    using SoundFingerprinting.Query;

    [TestFixture]
    public class ConfidenceCalculatorTest
    {
        private const double delta = 0.001;

        private readonly ConfidenceCalculator confidenceCalculator = new ConfidenceCalculator();

        /* Query: ----------
         * Track: -----
         */
        [TestCase(0, 10, 0, 5, 0, ExpectedResult = 0)]
        /* Query: ==--------
         * Track: ==---
         */
        [TestCase(0, 10, 0, 5, 2, ExpectedResult = 0.4)]
        /* Query: -==-------
         * Track: ==---
         */
        [TestCase(1, 10, 0, 5, 2, ExpectedResult = 0.4)]
        /* Query: ---==-----
         * Track: ==---
         */
        [TestCase(3, 10, 0, 5, 2, ExpectedResult = 0.4)]
        /* Query: --------==
         * Track: ==---
         */
        [TestCase(8, 10, 0, 5, 2, ExpectedResult = 1)]
        /* Query: ==--------
         * Track: -==--
         */
        [TestCase(0, 10, 1, 5, 2, ExpectedResult = 0.5)]
        /* Query: ---==-----
         * Track: -==--
         */
        [TestCase(3, 10, 1, 5, 2, ExpectedResult = 0.4)]
        /* Query: --------==
         * Track: -==--
         */
        [TestCase(8, 10, 1, 5, 2, ExpectedResult = 0.6666666666666666)]
        /* Query: ==--------
         * Track: ---==
         */
        [TestCase(0, 10, 3, 5, 2, ExpectedResult = 1)]
        /* Query: -==-------
         * Track: ---==
         */
        [TestCase(1, 10, 3, 5, 2, ExpectedResult = 0.6666666666666666)]
        /* Query: --==------
         * Track: ---==
         */
        [TestCase(2, 10, 3, 5, 2, ExpectedResult = 0.5)]
        /* Query: ---==-----
         * Track: ---==
         */
        [TestCase(3, 10, 3, 5, 2, ExpectedResult = 0.4)]
        /* Query: --------==
         * Track: ---==
         */
        [TestCase(8, 10, 3, 5, 2, ExpectedResult = 0.4)]
        /* Query: -=====----
         * Track: =====
         */
        [TestCase(1, 10, 0, 5, 5, ExpectedResult = 1)]
        public double QueryLongerThanTrack(
            double queryMatchStartsAt, double queryLength,
            double trackMatchStartsAt, double trackLength,
            double coverageWithPermittedGapsLength)
        {
            return confidenceCalculator.CalculateConfidence(
                queryMatchStartsAt,
                queryLength,
                trackMatchStartsAt,
                trackLength,
                coverageWithPermittedGapsLength,
                coverageWithPermittedGapsLength,
                coverageWithPermittedGapsLength);
        }

        /* Query: -----
         * Track: -----
         */
        [TestCase(0, 5, 0, 5, 0, ExpectedResult = 0)]
        /* Query: ==---
         * Track: ==---
         */
        [TestCase(0, 5, 0, 5, 2, ExpectedResult = 0.4)]
        /* Query: -==--
         * Track: ==---
         */
        [TestCase(1, 5, 0, 5, 2, ExpectedResult = 0.5)]
        /* Query: --==-
         * Track: ==---
         */
        [TestCase(2, 5, 0, 5, 2, ExpectedResult = 0.6666666666666666)]
        /* Query: ---==
         * Track: ==---
         */
        [TestCase(3, 5, 0, 5, 2, ExpectedResult = 1)]
        /* Query: ==---
         * Track: -==--
         */
        [TestCase(0, 5, 1, 5, 2, ExpectedResult = 0.5)]
        /* Query: -==--
         * Track: -==--
         */
        [TestCase(1, 5, 1, 5, 2, ExpectedResult = 0.4)]
        /* Query: --==-
         * Track: -==--
         */
        [TestCase(2, 5, 1, 5, 2, ExpectedResult = 0.5)]
        /* Query: ---==
         * Track: -==--
         */
        [TestCase(3, 5, 1, 5, 2, ExpectedResult = 0.6666666666666666)]
        /* Query: ==---
         * Track: ---==
         */
        [TestCase(0, 5, 3, 5, 2, ExpectedResult = 1)]
        /* Query: -==--
         * Track: ---==
         */
        [TestCase(1, 5, 3, 5, 2, ExpectedResult = 0.6666666666666666)]
        /* Query: --==-
         * Track: ---==
         */
        [TestCase(2, 5, 3, 5, 2, ExpectedResult = 0.5)]
        /* Query: ---==
         * Track: ---==
         */
        [TestCase(3, 5, 3, 5, 2, ExpectedResult = 0.4)]
        /* Query: =====
         * Track: =====
         */
        [TestCase(0, 5, 0, 5, 5, ExpectedResult = 1)]
        /* Query: ---=------
         * Track: =---------
         */
        [TestCase(3, 10, 0, 10, 1, ExpectedResult = 0.14285714285714285)]
        /* Query: ---==-----
         * Track: ==--------
         */
        [TestCase(3, 10, 0, 10, 2, ExpectedResult = 0.2857142857142857)]
        /* Query: ---===----
         * Track: ===-------
         */
        [TestCase(3, 10, 0, 10, 3, ExpectedResult = 0.42857142857142855)]
        /* Query: ---====---
         * Track: ====------
         */
        [TestCase(3, 10, 0, 10, 4, ExpectedResult = 0.5714285714285714)]
        /* Query: ---=====--
         * Track: =====-----
         */
        [TestCase(3, 10, 0, 10, 5, ExpectedResult = 0.7142857142857143)]
        /* Query: ---======-
         * Track: ======----
         */
        [TestCase(3, 10, 0, 10, 6, ExpectedResult = 0.8571428571428571)]
        /* Query: ---=======
         * Track: =======---
         */
        [TestCase(3, 10, 0, 10, 7, ExpectedResult = 1)]
        public double QueryAndTrackOfEqualLength(
            double queryMatchStartsAt, double queryLength,
            double trackMatchStartsAt, double trackLength,
            double coverageWithPermittedGapsLength)
        {
            return confidenceCalculator.CalculateConfidence(
                queryMatchStartsAt,
                queryLength,
                trackMatchStartsAt,
                trackLength,
                coverageWithPermittedGapsLength,
                coverageWithPermittedGapsLength,
                coverageWithPermittedGapsLength);
        }

        /* Query: -----
         * Track: ----------
         */
        [TestCase(0, 5, 0, 10, 0, ExpectedResult = 0)]
        /* Query: ==---
         * Track: ==--------
         */
        [TestCase(0, 5, 0, 10, 2, ExpectedResult = 0.4)]
        /* Query: -==--
         * Track: ==--------
         */
        [TestCase(1, 5, 0, 10, 2, ExpectedResult = 0.5)]
        /* Query: ---==
         * Track: ==--------
         */
        [TestCase(3, 5, 0, 10, 2, ExpectedResult = 1)]
        /* Query: ==---
         * Track: ---==-----
         */
        [TestCase(0, 5, 3, 10, 2, ExpectedResult = 0.4)]
        /* Query: --==-
         * Track: ---==-----
         */
        [TestCase(2, 5, 3, 10, 2, ExpectedResult = 0.4)]
        /* Query: ---==
         * Track: ---==-----
         */
        [TestCase(3, 5, 3, 10, 2, ExpectedResult = 0.4)]
        /* Query: ==---
         * Track: --------==
         */
        [TestCase(0, 5, 8, 10, 2, ExpectedResult = 1)]
        /* Query: -==--
         * Track: --------==
         */
        [TestCase(1, 5, 8, 10, 2, ExpectedResult = 0.6666666666666666)]
        /* Query: --==-
         * Track: --------==
         */
        [TestCase(2, 5, 8, 10, 2, ExpectedResult = 0.5)]
        /* Query: ---==
         * Track: --------==
         */
        [TestCase(3, 5, 8, 10, 2, ExpectedResult = 0.4)]
        /* Query: =====
         * Track: ---=====--
         */
        [TestCase(0, 5, 3, 10, 5, ExpectedResult = 1)]
        public double TrackLongerThanQuery(
            double queryMatchStartsAt, double queryLength,
            double trackMatchStartsAt, double trackLength,
            double coverageWithPermittedGapsLength)
        {
            return confidenceCalculator.CalculateConfidence(
                queryMatchStartsAt,
                queryLength,
                trackMatchStartsAt,
                trackLength,
                coverageWithPermittedGapsLength,
                coverageWithPermittedGapsLength,
                coverageWithPermittedGapsLength);
        }
    }
}
