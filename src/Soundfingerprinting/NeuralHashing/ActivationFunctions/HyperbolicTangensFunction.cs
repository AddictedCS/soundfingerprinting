// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;

namespace Soundfingerprinting.NeuralHashing.ActivationFunctions
{
    [Serializable]
    public class HyperbolicTangensFunction : IActivationFunction
    {
        private float alfa = 1.7159f;

        // Alfa property

        // Constructors
        public HyperbolicTangensFunction()
        {
        }

        public HyperbolicTangensFunction(float alfa)
        {
            // dividing alfa by two gives us the same function
            // as sigmoid function
            this.alfa = alfa;
        }

        public float Alfa
        {
            get { return alfa; }
            set { alfa = value; }
        }

        // Calculate function value

        #region IActivationFunction Members

        public float Output(float x)
        {
            return (float) (Math.Tanh(alfa*x));
        }

        // Calculate differential of the function value
        public float Derivative(float x)
        {
            float y = Output(x);
            return (alfa*(1 - y*y));
        }

        // Calculate differential of the function value
        // using function value as input
        public float Derivative2(float y)
        {
            return (alfa*(1 - y*y));
        }

        #endregion
    }
}