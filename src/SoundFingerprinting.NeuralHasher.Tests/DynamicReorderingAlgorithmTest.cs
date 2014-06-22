namespace SoundFingerprinting.NeuralHasher.Tests
{
    using System.Collections.Generic;

    using Encog.ML;
    using Encog.ML.Data.Basic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Tests;

    [TestClass]
    public class DynamicReorderingAlgorithmTest : AbstractTest
    {
        private readonly DynamicReorderingAlgorithm dynamicReorderingAlgorithm = new DynamicReorderingAlgorithm();

        [TestMethod]
        public void TestComputeAm()
        {
            var network = new Mock<IMLRegression>();
            var songSnippets = new List<double[][]>
                {
                    new[] { new double[] { 1 }, new double[] { 2 } },
                    new[] { new double[] { 3 }, new double[] { 4 } }
                };
            var networkOutput = new Queue<double[]>();
            networkOutput.Enqueue(new[] { 0, 1d });
            networkOutput.Enqueue(new[] { 0, 1d });
            networkOutput.Enqueue(new[] { 0.5, 0 });
            networkOutput.Enqueue(new[] { 0.5, 0 });
            network.Setup(n => n.Compute(It.IsAny<BasicMLData>())).Returns(() => new BasicMLData(networkOutput.Dequeue()));

            double[][] am = dynamicReorderingAlgorithm.ComputeAm(network.Object, songSnippets, 2);

            Assert.AreEqual(2, am.Length);
            AssertArraysAreEqual(new[] { 0, 1d }, am[0]);
            AssertArraysAreEqual(new[] { 0.5, 0 }, am[1]);
        }
    }
}
