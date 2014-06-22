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
        public void TestOutputIsNormalizedAccordingToSigmoidFunction()
        {
            var activationFunction = new ActivationSigmoid();
            double[] output = new double[] { 1, 0, 1, 0 };

            normalizeStrategy.NormalizeOutputInPlace(activationFunction, output);

            AssertArraysAreEqual(new double[] { 1, 0, 1, 0 }, output);
        }
    }
}
