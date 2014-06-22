namespace SoundFingerprinting.NeuralHasher.Utils
{
    using Encog.Engine.Network.Activation;

    internal interface INormalizeStrategy
    {
        void NormalizeOutputInPlace(IActivationFunction activationFunction, double[] output);

        void NormalizeOutputInPlace(IActivationFunction activationFunction, double[][] output);
        
        void NormalizeInputInPlace(IActivationFunction activationFunction, double[] input);

        void NormalizeInputInPlace(IActivationFunction activationFunction, double[][] input);
    }
}