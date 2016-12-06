namespace SoundFingerprinting.Tests.Unit.Query
{
    using NUnit.Framework;

    using SoundFingerprinting.Query;

    [TestFixture]
    public class ConfidenceCalculatorTest
    {
        private readonly ConfidenceCalculator confidenceCalculator = new ConfidenceCalculator();

        [Test]
        public void ShouldCalculateConfidence1()
        {
            // Query Length 2 mins
            // Source 30 sec (20 seconds of match available at the begining of the track)
            // 20 seconds match 

            double confidence = confidenceCalculator.CalculateConfidence(0d, 20d, 120d, 10d, 30d);

            Assert.AreEqual(1d, confidence, 0.001);
        }

        [Test]
        public void ShouldCalculatedConfidence2()
        {
            // Query Length 2 mins
            // Source 30 sec (30 seconds available in the middle of the track)
            // 30 seconds match

            double confidence = confidenceCalculator.CalculateConfidence(50d, 30d, 120d, 0d, 30d);

            Assert.AreEqual(1d, confidence, 0.001);
        }

        [Test]
        public void ShouldCalculateConfidence3()
        {
            // Query Length 2 mins
            // Source 30 sec (10 seconds of match available at the end of the track)
            // 10 seconds match

            double confidence = confidenceCalculator.CalculateConfidence(110d, 10d, 120d, 0, 30d);

            Assert.AreEqual(1d, confidence, 0.001);
        }

        [Test]
        public void ShouldCalculateConfidence4()
        {
            // Query Length 10 sec
            // Source 2 mins (10 seconds available in the middle)
            // 10 seconds match

            double confidence = confidenceCalculator.CalculateConfidence(50d, 10d, 10d, 50d, 120d);

            Assert.AreEqual(1d, confidence, 0.001);
        }

        [Test]
        public void ShouldCalculateConfidence5()
        {
            // Query Length 10 sec
            // Source 2 mins (10 seconds available in the middle)
            // 10 seconds match (1 sec missalignment to the left)

            double confidence = confidenceCalculator.CalculateConfidence(49d, 10d, 10d, 50d, 120d);

            Assert.AreEqual(1d, confidence, 0.001);
        }

        [Test]
        public void ShouldCalculateConfidence6()
        {
            // Query Length 10 sec
            // Source 2 mins (10 seconds available in the middle)
            // 10 seconds match (1 sec missalignment to the right)

            double confidence = confidenceCalculator.CalculateConfidence(0d, 10d, 10d, 50d, 120d);

            Assert.AreEqual(1d, confidence, 0.001);
        }

        [Test]
        public void ShouldCalculateConfidence7()
        {
            // Query Length 10 sec
            // Source 2 mins (5 seconds available in the begining)
            // 5 seconds match

            double confidence = confidenceCalculator.CalculateConfidence(0d, 5d, 10d, 115d, 120d);

            Assert.AreEqual(1d, confidence, 0.001);
        }

        [Test]
        public void ShouldCalculateConfidence()
        {
            // Query Length 10 sec
            // Source 2 mins (5 seconds available in the begining)
            // 5 seconds match

            double confidence = confidenceCalculator.CalculateConfidence(5d, 5d, 10d, 0d, 120d);

            Assert.AreEqual(1d, confidence, 0.001);
        }
    }
}
