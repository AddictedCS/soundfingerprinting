namespace Soundfingerprinting.Hashing.NeuralHashing
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Xml.Serialization;

    using Soundfingerprinting.Hashing.NeuralHashing.ActivationFunctions;
    using Soundfingerprinting.Hashing.NeuralHashing.Utils;

    /// <summary>
    ///   Neuron
    /// </summary>
    [Serializable]
    public class Neuron : ISerializable
    {
        #region Serialization Constants

        private const string INPUTS_COUNT = "InputsCount";
        private const string WEIGHT = "Weight";
        private const string BIAS = "Bias";
        private const string ACTIVATION_FUNCTION = "ActivationFunction";
        private const string SUM = "Sum";
        private const string OUTPUT = "Output";

        #endregion

        private static Random rand = new Random();

        /// <summary>
        ///   Random generator range.
        /// </summary>
        /// <remarks>
        ///   Sets the range of random generator. Affects initial values of neuron's weight.
        ///   Default value is [0, 1].
        /// </remarks>
        protected static FloatRange randRange = new FloatRange(-1.0f, 1.0f);

        private static Assembly assembly; /*Used at serialization*/

        #region Properties

        /// <summary>
        ///   Random number generator.
        /// </summary>
        /// <remarks>
        ///   The property allows to initialize random generator with a custom seed. The generator is
        ///   used for neuron's weights randomization.
        /// </remarks>
        public static Random RandGenerator
        {
            get { return rand; }
            set
            {
                if (value != null)
                {
                    rand = value;
                }
            }
        }

        /// <summary>
        ///   Random generator range.
        /// </summary>
        /// <remarks>
        ///   Sets the range of random generator. Affects initial values of neuron's weight.
        ///   Default value is [0, 1].
        /// </remarks>
        public static FloatRange RandRange
        {
            get { return randRange; }
            set
            {
                if (value != null)
                {
                    randRange = value;
                }
            }
        }


        /// <summary>
        ///   Inputs count
        /// </summary>
        public int InputsCount
        {
            get { return _inputsCount; }
            set
            {
                _inputsCount = Math.Max(1, value);
                _weights = new float[_inputsCount];
            }
        }

        /// <summary>
        ///   Threshold property
        /// </summary>
        public float Bias
        {
            get { return _bias; }
            set { _bias = value; }
        }

        /// <summary>
        ///   Activation function property
        /// </summary>
        [XmlIgnore]
        public IActivationFunction ActivationFunction
        {
            get { return _function; }
            set { _function = value; }
        }

        /// <summary>
        ///   Get output value of the neuron
        /// </summary>
        public float Output
        {
            get { return _output; }
        }

        /// <summary>
        ///   Get/Set weight value
        /// </summary>
        /// <param name = "index">Index of weight</param>
        /// <returns>Returns weight of the specified index</returns>
        public float this[int index]
        {
            get { return _weights[index]; }
            set { _weights[index] = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "inputs">Number of inputs of the neuron</param>
        public Neuron(int inputs)
        {
            _function = new SigmoidFunction();
            _inputsCount = Math.Max(1, inputs);
            _weights = new float[_inputsCount];
            Randomize();
        }

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "inputs">Number of inputs of the neuron</param>
        /// <param name = "function">Activation function of the neuron</param>
        public Neuron(int inputs, IActivationFunction function)
        {
            _function = function;
            _inputsCount = Math.Max(1, inputs);
            _weights = new float[_inputsCount];
            Randomize();
        }

        /// <summary>
        ///   Constructor called at Deserialization by Formatter class
        /// </summary>
        protected Neuron(SerializationInfo info, StreamingContext context)
        {
            _inputsCount = info.GetInt32(INPUTS_COUNT);
            if (_inputsCount > 0)
            {
                _weights = new float[_inputsCount];
                for (int i = 0; i < _inputsCount; i++)
                {
                    _weights[i] = info.GetSingle(WEIGHT + i.ToString(CultureInfo.InvariantCulture));
                }
            }
            _bias = info.GetSingle(BIAS);
            if (assembly == null)
                assembly = Assembly.Load(Assembly.GetAssembly(typeof (IActivationFunction)).FullName); /*Load Assembly With IActivationFunction type*/
            Type[] types = assembly.GetExportedTypes(); /*Get Public Types*/
            Type serializedType = Type.GetType(info.GetString(ACTIVATION_FUNCTION));

            foreach (Type type in types)
            {
                if (type.FullName == serializedType.FullName)
                {
                    ActivationFunction = (IActivationFunction) AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assembly.FullName, type.FullName);
                    break;
                }
            }
            _sum = info.GetSingle(SUM);
            _output = info.GetSingle(OUTPUT);
        }

        #endregion

        private readonly float _sum; // weighted input's sum
        private float _bias; /*0.0f initialized by the runtime*/

        [NonSerialized] private IActivationFunction _function = new SigmoidFunction();
        private int _inputsCount = 1;
        private float _output; // neuron's output value
        private float[] _weights; // synapses weights

        #region ISerializable Members

        /// <summary>
        ///   The Formatter  calls the ISerializable.GetObjectData  at serialization time and populates the
        ///   supplied SerializationInfo  with all the data required to represent the object.
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(INPUTS_COUNT, _inputsCount); //inputs
            for (int i = 0; i < _inputsCount; i++)
            {
                info.AddValue(WEIGHT + i.ToString(CultureInfo.InvariantCulture), _weights[i]); //all weights
            }
            info.AddValue(BIAS, _bias); //threshold
            info.AddValue(ACTIVATION_FUNCTION, _function.GetType()); //activation function
            info.AddValue(SUM, _sum); //sum
            info.AddValue(OUTPUT, _output); //output
        }

        #endregion

        /// <summary>
        ///   Compute the output value of the neuron
        /// </summary>
        /// <param name = "input">input array on which to compute</param>
        /// <returns>The output of the neuron</returns>
        public float Compute(float[] input)
        {
            // check for corrent input vector
            if (input.Length != _inputsCount)
                throw new ArgumentException("Wrong length of the input vector.");

            // initial sum value
            float sum = 0.0f;

            // compute weighted sum of inputs
            for (int i = 0; i < _inputsCount; i++)
            {
                sum += _weights[i]*input[i];
            }
            sum += _bias;

            // local variable to avoid mutlithreaded conflicts
            float output = _function.Output(sum);
            // assign output property as well (works correctly for single threaded usage)
            _output = output;

            return output;
        }

        /// <summary>
        ///   Randomize neuron.
        /// </summary>
        /// <remarks>
        ///   Initialize neuron's weights with random values within the range specified
        ///   by <see cref = "RandRange" />.
        /// </remarks>
        public void Randomize()
        {
            float d = randRange.Length;

            // randomize weights
            for (int i = 0; i < _inputsCount; i++)
                _weights[i] = (float) rand.NextDouble()*d + randRange.Min;
            _bias = (float) rand.NextDouble()*(randRange.Length) + randRange.Min;
        }

        /// <summary>
        ///   Update weights of a specific neuron. Method written in order to avoid Get/Set calles. The update is performed by summing +=.
        /// </summary>
        /// <param name = "updates">Array used to udpate the values</param>
        public void UpdateWeights(float[] updates)
        {
            for (int i = 0, n = _inputsCount; i < n; i++)
            {
                _weights[i] += updates[i];
            }
        }
    }
}