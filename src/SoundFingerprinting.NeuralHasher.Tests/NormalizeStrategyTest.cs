namespace SoundFingerprinting.NeuralHasher.Tests
{
    using Encog.Engine.Network.Activation;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.NeuralHasher.Utils;
    using SoundFingerprinting.Tests;

    [TestClass]
    public class NormalizeStrategyTest : AbstractTest
    {
        private readonly INormalizeStrategy normalizeStrategy = new NormalizeStrategy();

        [TestMethod]
        public void TestOutputIsNomalizedAccordingToTANHFunction()
        {
            var activationFunction = new ActivationTANH();
            double[] output = new double[] { 1, 0, 1, 0 };

            normalizeStrategy.NormalizeOutputInPlace(activationFunction, output);

            AssertArraysAreEqual(new double[] { 1, -1, 1, -1 }, output);
        }

        [TestMethod]
        public void TestInputIsNormalizedAccordingToTANHFunction()
        {
            var activationFunction = new ActivationTANH();
            double[] input = new[] { -1.3, -0.7, 0.1, 0.3, 1.1, 0.5 };

            normalizeStrategy.NormalizeInputInPlace(activationFunction, input);

            AssertArraysAreEqual(new[] { -1, -0.7, 0.1, 0.3, 1, 0.5 }, input);
        }
    }
}
