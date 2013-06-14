namespace SoundFingerprinting.Hashing.NeuralHashing.ActivationFunctions
{
    using System;

    [Serializable]
    public class BipolarSigmoidFunction : IActivationFunction
    {
        private float alfa = 2;

        public BipolarSigmoidFunction()
        {
            // no op
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


        #region IActivationFunction Members

        public float Output(float x)
        {
            return (float)((1.0f / (1 + Math.Exp(-alfa * x))) - 0.5);
        }

        // Calculate differential of the function value
        public float Derivative(float x)
        {
            float y = Output(x);

            return (float)(alfa * (0.25 - y * y));
        }

        // Calculate differential of the function value
        // using function value as input
        public float Derivative2(float y)
        {
            return (float)(alfa * (0.25 - y * y));
        }

        #endregion
    }
}