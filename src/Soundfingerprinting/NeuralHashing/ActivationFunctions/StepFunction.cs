// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;

namespace Soundfingerprinting.NeuralHashing.ActivationFunctions
{
    [Serializable]
    public class StepFunction : IActivationFunction
    {
        private float _threshold;

        // Threshold property

        public StepFunction()
        {
        }


        public StepFunction(float threshold)
        {
            _threshold = threshold;
        }

        public float Threshold
        {
            get { return _threshold; }
            set { _threshold = value; }
        }

        #region IActivationFunction Members

        public float Output(float input)
        {
            return input > _threshold ? 1.0f : 0.0f;
        }

        public float Derivative(float input)
        {
            return 0;
        }

        public float Derivative2(float input)
        {
            return 0;
        }

        #endregion
    }
}