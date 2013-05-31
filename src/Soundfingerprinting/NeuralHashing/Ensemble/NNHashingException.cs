namespace Soundfingerprinting.NeuralHashing.Ensemble
{
    using System;
    using System.Runtime.Serialization;

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