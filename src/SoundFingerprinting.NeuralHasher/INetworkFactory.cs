namespace SoundFingerprinting.NeuralHasher
{
    using System.IO;

    using Encog.Engine.Network.Activation;

    public interface INetworkFactory
    {
        Network Create(string pathToFilename);

        Network Create(Stream stream);

        Network Create<T>(params int[] hiddenNeurons) where T : IActivationFunction, new();
    }
}