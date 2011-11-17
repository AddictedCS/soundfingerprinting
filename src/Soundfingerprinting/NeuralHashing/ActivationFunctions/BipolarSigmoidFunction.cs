// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;

namespace Soundfingerprinting.NeuralHashing.ActivationFunctions
{
    [Serializable]
    public class BipolarSigmoidFunction : IActivationFunction
    {
        private float alfa = 2;

        // Alfa property

        // Constructors
        public BipolarSigmoidFunction()
        {
        }

        public BipolarSigmoidFunction(float alfa)
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
            return (float) ((1.0f/(1 + Math.Exp(-alfa*x))) - 0.5);
        }

        // Calculate differential of the function value
        public float Derivative(float x)
        {
            float y = Output(x);

            return (float) (alfa*(0.25 - y*y));
        }

        // Calculate differential of the function value
        // using function value as input
        public float Derivative2(float y)
        {
            return (float) (alfa*(0.25 - y*y));
        }

        #endregion
    }
}