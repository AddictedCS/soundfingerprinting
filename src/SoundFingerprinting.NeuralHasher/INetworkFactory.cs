namespace SoundFingerprinting.NeuralHasher
{
    using System.IO;

    using Encog.Engine.Network.Activation;

    public interface INetworkFactory
    {
        Network LoadNetworkFromFile(string pathToFilename);

        Network LoadNetworkFromStream(Stream stream);

        Network Create<T>(T activationFunction, params int[] hiddenNeurons) where T : IActivationFunction;
    }
}