namespace SoundFingerprinting.DAO
{
    using System.Threading;
    using ProtoBuf;

    [ProtoContract]
    public class LongModelReferenceProvider : IModelReferenceProvider
    {
        [ProtoMember(1)]
        private long referenceCounter;

        public LongModelReferenceProvider(long referenceCounter = 0)
        {
            this.referenceCounter = referenceCounter;
        }

        private LongModelReferenceProvider()
        {
            // left for protobuf
        }

        public IModelReference Next()
        {
            long next = Interlocked.Increment(ref referenceCounter);
            return new ModelReference<long>(next);
        }
    }
}