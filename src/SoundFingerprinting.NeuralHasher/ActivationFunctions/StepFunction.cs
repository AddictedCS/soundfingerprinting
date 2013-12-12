namespace SoundFingerprinting.NeuralHasher.ActivationFunctions
{
    using System;

    [Serializable]
    public class StepFunction : IActivationFunction
    {
        private float threshold;

        public StepFunction(float threshold)
        {
            this.threshold = threshold;
        }

        public float Threshold
        {
            get { return threshold; }
            set { threshold = value; }
        }

        public float Output(float input)
        {
            return input > threshold ? 1.0f : 0.0f;
        }

        public float Derivative(float input)
        {
            return 0;
        }

        public float Derivative2(float input)
        {
            return 0;
        }
    }
}