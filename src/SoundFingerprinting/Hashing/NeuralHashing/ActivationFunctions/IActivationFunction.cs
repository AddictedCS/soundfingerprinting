namespace SoundFingerprinting.Hashing.NeuralHashing.ActivationFunctions
{
    /// <summary>
    ///   ActivationFunction interface
    /// </summary>
    public interface IActivationFunction
    {
        // Calculate function value
        float Output(float input);

        // Calculate differential of the function value
        float Derivative(float input);

        // Calculate differential of the function value
        // using function value as input
        float Derivative2(float input);
    }
}