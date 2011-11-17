// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;

namespace Soundfingerprinting.NeuralHashing.ActivationFunctions
{
    [Serializable]
    public class SigmoidFunction : IActivationFunction
    {
        private float alfa = 2f;

        // Alfa property

        // Constructors
        public SigmoidFunction()
        {
        }

        public SigmoidFunction(float alfa)
        {
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
            return (float) (1/(1 + Math.Exp(-alfa*x)));
        }

        // Calculate differential of the function value
        public float Derivative(float x)
        {
            float y = Output(x);

            return (alfa*y*(1 - y));
        }

        // Calculate differential of the function value
        // using function value as input
        public float Derivative2(float y)
        {
            return (alfa*y*(1 - y));
        }

        #endregion
    }
}