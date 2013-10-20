namespace SoundFingerprinting.NeuralHasher.ActivationFunctions
{
    using System;

    [Serializable]
    public class HyperbolicTangensFunction : IActivationFunction
    {
        private float alfa = 1.7159f;

        public HyperbolicTangensFunction()
        {
            // no op
        }

        public HyperbolicTangensFunction(float alfa)
        {
            // dividing alfa by two gives us the same function
            // as sigmoid function
            this.alfa = alfa;
        }

        public float Alfa
        {
            get
            {
                return alfa;
            }
            set
            {
                alfa = value;
            }
        }

        public float Output(float x)
        {
            return (float)Math.Tanh(alfa * x);
        }

        // Calculate differential of the function value
        public float Derivative(float x)
        {
            float y = Output(x);
            return alfa * (1 - (y * y));
        }

        // Calculate differential of the function value
        // using function value as input
        public float Derivative2(float y)
        {
            return alfa * (1 - (y * y));
        }
    }
}