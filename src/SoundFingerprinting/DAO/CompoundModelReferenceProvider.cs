namespace SoundFingerprinting.DAO
{
    using System.Threading;
    using ProtoBuf;

    [ProtoContract(SkipConstructor = true)]
    public class CompoundModelReferenceProvider : IModelReferenceProvider
    {
        [ProtoMember(1)]
        private readonly string prefix;

        [ProtoMember(2)]
        private int referenceCounter;

        public CompoundModelReferenceProvider(string prefix)
        {
            this.prefix = prefix;
        }

        public IModelReference Next()
        {
            int next = Interlocked.Increment(ref referenceCounter);
            return new CompoundModelReference<string>(prefix, new ModelReference<int>(next));
        }

        public static CompoundModelReference<string> Null { get; } = new CompoundModelReference<string>(string.Empty, new ModelReference<int>(0));
    }
}