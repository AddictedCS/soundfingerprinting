// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Runtime.Serialization;

namespace Soundfingerprinting.NeuralHashing.NeuralTrainer
{
    /// <summary>
    ///   NetTrainer Exception
    /// </summary>
    [Serializable]
    public class NetTrainerException : Exception
    {
        public NetTrainerException()
        {
        }

        public NetTrainerException(string message) : base(message)
        {
        }

        public NetTrainerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NetTrainerException(SerializationInfo info, StreamingContext ctx) : base(info, ctx)
        {
        }
    }
}