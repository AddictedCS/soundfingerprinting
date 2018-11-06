namespace SoundFingerprinting.DAO
{
    using System.Threading;
    using ProtoBuf;

    [ProtoContract]
    public class IntModelReferenceProvider : IModelReferenceProvider
    {
        [ProtoMember(1)]
        private int referenceCounter;

        public IModelReference Next()
        {
            int reference = Interlocked.Increment(ref referenceCounter);
            return new ModelReference<int>(reference);
        }
    }
}