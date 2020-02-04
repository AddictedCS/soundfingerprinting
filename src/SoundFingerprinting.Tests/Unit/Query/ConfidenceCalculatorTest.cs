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
        [TestCase(0, 0, 10, 0, 5, ExpectedResult = 0)]
        /* Query: ==--------
         * Track: ==---
         */
        [TestCase(0, 2, 10, 0, 5, ExpectedResult = 0.4)]
        /* Query: -==-------
         * Track: ==---
         */
        [TestCase(1, 2, 10, 0, 5, ExpectedResult = 0.4)]
        /* Query: ---==-----
         * Track: ==---
         */
        [TestCase(3, 2, 10, 0, 5, ExpectedResult = 0.4)]
        /* Query: --------==
         * Track: ==---
         */
        [TestCase(8, 2, 10, 0, 5, ExpectedResult = 1)]
        /* Query: ==--------
         * Track: -==--
         */
        [TestCase(0, 2, 10, 1, 5, ExpectedResult = 0.5)]
        /* Query: ---==-----
         * Track: -==--
         */
        [TestCase(3, 2, 10, 1, 5, ExpectedResult = 0.4)]
        /* Query: --------==
         * Track: -==--
         */
        [TestCase(8, 2, 10, 1, 5, ExpectedResult = 0.6666666666666666)]
        /* Query: ==--------
         * Track: ---==
         */
        [TestCase(0, 2, 10, 3, 5, ExpectedResult = 1)]
        /* Query: -==-------
         * Track: ---==
         */
        [TestCase(1, 2, 10, 3, 5, ExpectedResult = 0.6666666666666666)]
        /* Query: --==------
         * Track: ---==
         */
        [TestCase(2, 2, 10, 3, 5, ExpectedResult = 0.5)]
        /* Query: ---==-----
         * Track: ---==
         */
        [TestCase(3, 2, 10, 3, 5, ExpectedResult = 0.4)]
        /* Query: --------==
         * Track: ---==
         */
        [TestCase(8, 2, 10, 3, 5, ExpectedResult = 0.4)]
        /* Query: -=====----
         * Track: =====
         */
        [TestCase(1, 5, 10, 0, 5, ExpectedResult = 1)]

        public double QueryLongerThanTrack(double queryMatchStartsAt, double coverageLength, double queryLength,
            double trackMatchStartsAt, double trackLength)
        {
            return confidenceCalculator.CalculateConfidence(
                queryMatchStartsAt, queryLength, trackMatchStartsAt, trackLength, coverageLength, coverageLength);
        }

        /* Query: -----
         * Track: -----
         */
        [TestCase(0, 0, 5, 0, 5, ExpectedResult = 0)]
        /* Query: ==---
         * Track: ==---
         */
        [TestCase(0, 2, 5, 0, 5, ExpectedResult = 0.4)]
        /* Query: -==--
         * Track: ==---
         */
        [TestCase(1, 2, 5, 0, 5, ExpectedResult = 0.5)]
        /* Query: --==-
         * Track: ==---
         */
        [TestCase(2, 2, 5, 0, 5, ExpectedResult = 0.6666666666666666)]
        /* Query: ---==
         * Track: ==---
         */
        [TestCase(3, 2, 5, 0, 5, ExpectedResult = 1)]
        /* Query: ==---
         * Track: -==--
         */
        [TestCase(0, 2, 5, 1, 5, ExpectedResult = 0.5)]
        /* Query: -==--
         * Track: -==--
         */
        [TestCase(1, 2, 5, 1, 5, ExpectedResult = 0.4)]
        /* Query: --==-
         * Track: -==--
         */
        [TestCase(2, 2, 5, 1, 5, ExpectedResult = 0.5)]
        /* Query: ---==
         * Track: -==--
         */
        [TestCase(3, 2, 5, 1, 5, ExpectedResult = 0.6666666666666666)]
        /* Query: ==---
         * Track: ---==
         */
        [TestCase(0, 2, 5, 3, 5, ExpectedResult = 1)]
        /* Query: -==--
         * Track: ---==
         */
        [TestCase(1, 2, 5, 3, 5, ExpectedResult = 0.6666666666666666)]
        /* Query: --==-
         * Track: ---==
         */
        [TestCase(2, 2, 5, 3, 5, ExpectedResult = 0.5)]
        /* Query: ---==
         * Track: ---==
         */
        [TestCase(3, 2, 5, 3, 5, ExpectedResult = 0.4)]
        /* Query: =====
         * Track: =====
         */
        [TestCase(0, 5, 5, 0, 5, ExpectedResult = 1)]
        /* Query: ---=------
         * Track: =---------
         */
        [TestCase(3, 1, 10, 0, 10, ExpectedResult = 0.14285714285714285)]
        /* Query: ---==-----
         * Track: ==--------
         */
        [TestCase(3, 2, 10, 0, 10, ExpectedResult = 0.2857142857142857)]
        /* Query: ---===----
         * Track: ===-------
         */
        [TestCase(3, 3, 10, 0, 10, ExpectedResult = 0.42857142857142855)]
        /* Query: ---====---
         * Track: ====------
         */
        [TestCase(3, 4, 10, 0, 10, ExpectedResult = 0.5714285714285714)]
        /* Query: ---=====--
         * Track: =====-----
         */
        [TestCase(3, 5, 10, 0, 10, ExpectedResult = 0.7142857142857143)]
        /* Query: ---======-
         * Track: ======----
         */
        [TestCase(3, 6, 10, 0, 10, ExpectedResult = 0.8571428571428571)]
        /* Query: ---=======
         * Track: =======---
         */
        [TestCase(3, 7, 10, 0, 10, ExpectedResult = 1)]
        public double QueryAndTrackOfEqualLength(double queryMatchStartsAt, double coverageLength, double queryLength,
            double trackMatchStartsAt, double trackLength)
        {
            return confidenceCalculator.CalculateConfidence(
                queryMatchStartsAt, queryLength, trackMatchStartsAt, trackLength, coverageLength, coverageLength);
        }

        /* Query: -----
         * Track: ----------
         */
        [TestCase(0, 0, 5, 0, 10, ExpectedResult = 0)]
        /* Query: ==---
         * Track: ==--------
         */
        [TestCase(0, 2, 5, 0, 10, ExpectedResult = 0.4)]
        /* Query: -==--
         * Track: ==--------
         */
        [TestCase(1, 2, 5, 0, 10, ExpectedResult = 0.5)]
        /* Query: ---==
         * Track: ==--------
         */
        [TestCase(3, 2, 5, 0, 10, ExpectedResult = 1)]
        /* Query: ==---
         * Track: ---==-----
         */
        [TestCase(0, 2, 5, 3, 10, ExpectedResult = 0.4)]
        /* Query: --==-
         * Track: ---==-----
         */
        [TestCase(2, 2, 5, 3, 10, ExpectedResult = 0.4)]
        /* Query: ---==
         * Track: ---==-----
         */
        [TestCase(3, 2, 5, 3, 10, ExpectedResult = 0.4)]
        /* Query: ==---
         * Track: --------==
         */
        [TestCase(0, 2, 5, 8, 10, ExpectedResult = 1)]
        /* Query: -==--
         * Track: --------==
         */
        [TestCase(1, 2, 5, 8, 10, ExpectedResult = 0.6666666666666666)]
        /* Query: --==-
         * Track: --------==
         */
        [TestCase(2, 2, 5, 8, 10, ExpectedResult = 0.5)]
        /* Query: ---==
         * Track: --------==
         */
        [TestCase(3, 2, 5, 8, 10, ExpectedResult = 0.4)]
        /* Query: =====
         * Track: ---=====--
         */
        [TestCase(0, 5, 5, 3, 10, ExpectedResult = 1)]
        public double TrackLongerThanQuery(double queryMatchStartsAt, double coverageLength, double queryLength,
            double trackMatchStartsAt, double trackLength)
        {
            return confidenceCalculator.CalculateConfidence(
                queryMatchStartsAt, queryLength, trackMatchStartsAt, trackLength, coverageLength, coverageLength);
        }

        [Test]
        public void ShouldCalculateConfidence1()
        {
            // Query Length 2 mins
            // Source 30 sec (20 seconds of match available at the beginning of the track)
            // 20 seconds match

            double confidence = confidenceCalculator.CalculateConfidence(0d, 120d, 10d, 30d, 20d, 20d);

            Assert.AreEqual(1d, confidence, delta);
        }

        [Test]
        public void ShouldCalculateConfidence2()
        {
            // Query Length 2 mins
            // Source 30 sec (30 seconds available in the middle of the track)
            // 30 seconds match

            double confidence = confidenceCalculator.CalculateConfidence(50d, 120d, 0d, 30d, 30d, 30d);

            Assert.AreEqual(1d, confidence, delta);
        }

        [Test]
        public void ShouldCalculateConfidence3()
        {
            // Query Length 2 mins
            // Source 30 sec (10 seconds of match available at the end of the track)
            // 10 seconds match

            double confidence = confidenceCalculator.CalculateConfidence(110d, 120d, 0, 30d, 10d, 10d);

            Assert.AreEqual(1d, confidence, delta);
        }

        [Test]
        public void ShouldCalculateConfidence4()
        {
            // Query Length 10 sec
            // Source 2 mins (10 seconds available in the middle)
            // 10 seconds match

            double confidence = confidenceCalculator.CalculateConfidence(50d, 10d, 50d, 120d, 10d, 10d);

            Assert.AreEqual(1d, confidence, delta);
        }

        [Test]
        public void ShouldCalculateConfidence5()
        {
            // Query Length 10 sec
            // Source 2 mins (10 seconds available in the middle)
            // 10 seconds match (1 sec misalignment to the left)

            double confidence = confidenceCalculator.CalculateConfidence(49d, 10d, 50d, 120d, 10d, 10d);

            Assert.AreEqual(1d, confidence, delta);
        }

        [Test]
        public void ShouldCalculateConfidence6()
        {
            // Query Length 10 sec
            // Source 2 mins (10 seconds available in the middle)
            // 10 seconds match (1 sec misalignment to the right)

            double confidence = confidenceCalculator.CalculateConfidence(0d, 10d, 50d, 120d, 10d, 10d);

            Assert.AreEqual(1d, confidence, delta);
        }

        [Test]
        public void ShouldCalculateConfidence7()
        {
            // Query Length 10 sec
            // Source 2 mins (5 seconds available in the beginning)
            // 5 seconds match

            double confidence = confidenceCalculator.CalculateConfidence(0d, 10d, 115d, 120d, 5d, 5d);

            Assert.AreEqual(1d, confidence, delta);
        }

        [Test]
        public void ShouldCalculateConfidence()
        {
            // Query Length 10 sec
            // Source 2 mins (5 seconds available in the beginning)
            // 5 seconds match

            double confidence = confidenceCalculator.CalculateConfidence(5d, 10d, 0d, 120d, 5d, 5d);

            Assert.AreEqual(1d, confidence, delta);
        }
    }
}
