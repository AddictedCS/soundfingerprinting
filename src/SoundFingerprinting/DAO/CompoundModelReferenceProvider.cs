namespace SoundFingerprinting.DAO
{
    using System.Threading;
    using ProtoBuf;

    [ProtoContract]
    public class CompoundModelReferenceProvider : IModelReferenceProvider
    {
        [ProtoMember(1)]
        private readonly string prefix;
        
        [ProtoMember(2)]
        private int referenceCounter;

        private CompoundModelReferenceProvider()
        {
            // left for proto-buf
        }

        public CompoundModelReferenceProvider(string prefix)
        {
            this.prefix = prefix;
        }
        
        public IModelReference Next()
        {
            int next = Interlocked.Increment(ref referenceCounter);
            return new CompoundModelReference(prefix, next);
        }
    }
}