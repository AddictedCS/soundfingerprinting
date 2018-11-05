namespace SoundFingerprinting.DAO
{
    using System.Threading;
    using ProtoBuf;
    
    [ProtoContract]
    public class UIntModelReferenceProvider : IModelReferenceProvider
    {
        [ProtoMember(1)]
        private long referenceCounter;

        public IModelReference Next()
        {
            var next = (uint)Interlocked.Increment(ref referenceCounter);
            return new ModelReference<uint>(next);
        }
    }
}