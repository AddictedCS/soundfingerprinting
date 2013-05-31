namespace Soundfingerprinting.NeuralHashing
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using Soundfingerprinting.NeuralHashing.ActivationFunctions;

    /// <summary>
    ///   Layer - represent a collection of neurons
    /// </summary>
    [Serializable]
    public class Layer
    {
        private readonly int _inputsCount; // inputs count of the layer
        private readonly Neuron[] _neurons;
        private readonly int _neuronsCount; // neurons count in the layer

        [NonSerialized] private IActivationFunction _function; // activation function of the layer
        private float[] _output;

        #region Propreties

        /// <summary>
        ///   Inputs count property
        /// </summary>
        public int InputsCount
        {
            get { return _inputsCount; }
        }

        /// <summary>
        ///   Neurons count property
        /// </summary>
        public int NeuronsCount
        {
            get { return _neuronsCount; }
        }

        /// <summary>
        ///   Activation function property
        /// </summary>
        [XmlIgnore]
        public IActivationFunction ActivationFunction
        {
            get { return _function; }
            set
            {
                _function = value;

                for (int i = 0; i < _neuronsCount; i++)
                    _neurons[i].ActivationFunction = value;
            }
        }

        /// <summary>
        ///   Get neuron at the specified index
        /// </summary>
        public Neuron this[int index]
        {
            get { return (_neurons[index]); }
        }

        /// <summary>
        ///   Get layer output
        /// </summary>
        public ReadOnlyCollection<float> Output
        {
            get
            {
                if (_output == null)
                    return null;
                return new ReadOnlyCollection<float>(_output);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///   Constructor, all nonspecified parameters are like in default constructor
        /// </summary>
        /// <param name = "neuronsCount">Number of neurons in the layer</param>
        /// <param name = "inputsCount">Number of inputs per each neuron in the layer</param>
        public Layer(int neuronsCount, int inputsCount)
        {
            _inputsCount = Math.Max(1, inputsCount);
            _neuronsCount = Math.Max(1, neuronsCount);
            // create collection of neurons
            _neurons = new Neuron[_neuronsCount];
            _function = new SigmoidFunction();
            for (int i = 0; i < neuronsCount; i++)
                _neurons[i] = new Neuron(inputsCount);
        }

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "neuronsCount">Number of neurons in the layer</param>
        /// <param name = "inputsCount">Number of inputs per each neuron in the layer</param>
        /// <param name = "function">Activation function for the whole layer</param>
        public Layer(int neuronsCount, int inputsCount, IActivationFunction function)
        {
            _inputsCount = Math.Max(1, inputsCount);
            _neuronsCount = Math.Max(1, neuronsCount);
            _function = function;
            _neurons = new Neuron[_neuronsCount];
            for (int i = 0; i < neuronsCount; i++)
                _neurons[i] = new Neuron(inputsCount, function);
        }

        #endregion

        /// <summary>
        ///   Compute the output value of the layer
        /// </summary>
        /// <param name = "input">Input array of the layer</param>
        /// <returns>Output array of the layer</returns>
        public float[] Compute(float[] input)
        {
            // local variable to avoid mutlithread conflicts
            float[] output = new float[_neuronsCount];

            // the contention is too high for parallel computing
            for (int i = 0; i < _neuronsCount; i++)
            {
                output[i] = _neurons[i].Compute(input); // compute each neuron
            }
            // assign output property as well (works correctly for single threaded usage)
            _output = output;

            return output;
        }

        /// <summary>
        ///   Sets Activation function for the whole layer
        /// </summary>
        /// <param name = "function">Activation function to set</param>
        public void SetActivationFunction(IActivationFunction function)
        {
            _function = function;
            for (int i = 0; i < _neuronsCount; i++)
            {
                _neurons[i].ActivationFunction = function;
            }
        }

        /// <summary>
        ///   Randomizes each neuron of the layer
        /// </summary>
        public void Randomize()
        {
            foreach (Neuron neuron in _neurons)
                neuron.Randomize();
        }

        [OnDeserialized]
        private void SetActivationFunctionOnDeserialization(StreamingContext context)
        {
            if (_neurons.Length > 0)
                ActivationFunction = _neurons[0].ActivationFunction;
        }
    }
}