namespace SoundFingerprinting.NeuralHasher
{
    using Encog.Engine.Network.Activation;

    public class NetworkConfiguration
    {
        public IActivationFunction ActivationFunction { get; set; }

        public int HiddenLayerCount { get; set; }

        public int OutputCount { get; set; }
    }
}