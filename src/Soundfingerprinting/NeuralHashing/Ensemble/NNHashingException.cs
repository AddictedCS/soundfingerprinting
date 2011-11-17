// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Runtime.Serialization;

namespace Soundfingerprinting.NeuralHashing.Ensemble
{
    /// <summary>
    ///   Neural Network Hashing exception
    /// </summary>
    [Serializable]
    internal class NNHashingException : Exception
    {
        public NNHashingException()
        {
        }

        public NNHashingException(string message) : base(message)
        {
        }

        public NNHashingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NNHashingException(SerializationInfo info, StreamingContext ctx) : base(info, ctx)
        {
        }
    }
}