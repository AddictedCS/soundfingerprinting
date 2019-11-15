namespace SoundFingerprinting.DAO
{
    using System.Threading;
    using ProtoBuf;
    
    [ProtoContract]
    public class UIntModelReferenceProvider : IModelReferenceProvider
    {
        [ProtoMember(1)]
        private long referenceCounter;

        public UIntModelReferenceProvider(long referenceCounter = 0)
        {
            this.referenceCounter = referenceCounter;
        }

        private UIntModelReferenceProvider()
        {
            // left for protobuf
        }
        
        public long Current => referenceCounter;

        public IModelReference Next()
        {
            var next = (uint)Interlocked.Increment(ref referenceCounter);
            return new ModelReference<uint>(next);
        }
    }
}