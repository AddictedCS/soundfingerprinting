namespace SoundFingerprinting.DAO
{
    using System.Threading;
    using ProtoBuf;
    
    [ProtoContract]
    public class UIntModelReferenceProvider : IModelReferenceProvider
    {
        [ProtoMember(1)]
        private long referenceCounter;
        
        [ProtoMember(2)]
        private long maxAllowedReference = int.MaxValue;

        public UIntModelReferenceProvider(long referenceCounter = 0, long maxAllowedReference = int.MaxValue)
        {
            if (referenceCounter > maxAllowedReference)
            {
                throw new ModelReferenceMaxAllowedValueExceededException(referenceCounter);
            }
            
            this.referenceCounter = referenceCounter;
            this.maxAllowedReference = maxAllowedReference;
        }

        private UIntModelReferenceProvider()
        {
            // left for protobuf
        }
        
        public long Current => referenceCounter;

        public IModelReference Next()
        {
            long increment = Interlocked.Increment(ref referenceCounter);
            if (increment > maxAllowedReference)
            {
                throw new ModelReferenceMaxAllowedValueExceededException(increment);
            }
            
            var next = (uint)increment;
            return new ModelReference<uint>(next);
        }
    }
}