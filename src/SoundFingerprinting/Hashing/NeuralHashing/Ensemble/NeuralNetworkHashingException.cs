namespace SoundFingerprinting.Hashing.NeuralHashing.Ensemble
{
    using System;
    using System.Runtime.Serialization;

    internal class NeuralNetworkHashingException : Exception
    {
        public NeuralNetworkHashingException()
        {
        }

        public NeuralNetworkHashingException(string message) : base(message)
        {
        }

        public NeuralNetworkHashingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NeuralNetworkHashingException(SerializationInfo info, StreamingContext ctx) : base(info, ctx)
        {
        }
    }
}