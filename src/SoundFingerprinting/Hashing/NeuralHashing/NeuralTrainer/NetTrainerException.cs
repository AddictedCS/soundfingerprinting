namespace SoundFingerprinting.Hashing.NeuralHashing.NeuralTrainer
{
    using System;
    using System.Runtime.Serialization;

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