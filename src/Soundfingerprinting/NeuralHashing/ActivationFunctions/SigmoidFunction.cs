namespace Soundfingerprinting.NeuralHashing.ActivationFunctions
{
    using System;

    [Serializable]
    public class SigmoidFunction : IActivationFunction
    {
        private float alfa = 2f;

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