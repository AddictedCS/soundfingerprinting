// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
namespace Soundfingerprinting.NeuralHashing.ActivationFunctions
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