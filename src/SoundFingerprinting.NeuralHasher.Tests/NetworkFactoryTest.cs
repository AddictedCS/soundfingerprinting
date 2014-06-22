namespace SoundFingerprinting.NeuralHasher.Tests
{
    using System.IO;

    using Encog.Engine.Network.Activation;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Tests;

    [TestClass]
    public class NetworkFactoryTest : AbstractTest 
    {
        private readonly NetworkFactory networkFactory = new NetworkFactory();

        [TestMethod]
        public void TestNetworkIsCreatedSuccessfully()
        {
            Network network = networkFactory.Create(new ActivationTANH(), 10, 20, 10);
            
            Assert.AreEqual(3, network.LayerCount);
            Assert.AreEqual(10, network.InputCount);
            Assert.AreEqual(10, network.OutputCount);
        }

        [TestMethod]
        public void TestNetworkIsCreatedFromFileSuccessfully()
        {
            Network network = networkFactory.Create(new ActivationTANH(), 8096, 41, 10);
            network.Save("network.encog");
            Assert.IsTrue(File.Exists("network.encog"));

            Network fromFile = networkFactory.LoadNetworkFromFile("network.encog");

            Assert.AreEqual(3, fromFile.LayerCount);
            Assert.AreEqual(8096, fromFile.InputCount);
            Assert.AreEqual(10, fromFile.OutputCount);
        }
    }
}
