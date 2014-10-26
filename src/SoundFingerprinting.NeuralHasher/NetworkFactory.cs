namespace SoundFingerprinting.NeuralHasher
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using Encog.Engine.Network.Activation;
    using Encog.Neural.Networks.Layers;

    public class NetworkFactory : INetworkFactory
    {
        private readonly Random random = new Random((int)DateTime.Now.Ticks);

        public Network Create<T>(T activationFunction, params int[] hiddenNeurons) where T : IActivationFunction
        {
            Network network = new Network();
            for (int i = 0; i < hiddenNeurons.Length; i++)
            {
                bool isLast = i == hiddenNeurons.Length - 1;
                network.AddLayer(new BasicLayer(activationFunction, !isLast, hiddenNeurons[i]));
            }
 
            network.Structure.FinalizeStructure();
            network.Reset(random.Next());
            return network;
        }

        public Network LoadNetworkFromFile(string pathToFilename)
        {
            using (FileStream stream = new FileStream(pathToFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return LoadNetworkFromStream(stream);
            }
        }

        public Network LoadNetworkFromStream(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            return (Network)formatter.Deserialize(stream);
        }
    }
}
