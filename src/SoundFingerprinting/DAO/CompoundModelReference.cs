namespace SoundFingerprinting.DAO
{
    using ProtoBuf;

    [ProtoContract]
    public class CompoundModelReference : IModelReference<string>
    {
        [ProtoMember(1)]
        private readonly string prefix;
        
        [ProtoMember(2)]
        private readonly int id;

        private CompoundModelReference()
        {
            // left for proto-buf
        }
        
        public CompoundModelReference(string prefix, int id)
        {
            this.prefix = prefix;
            this.id = id;
        }
        
        object IModelReference.Id => Id;

        public string Id => $"{prefix}_{id}";
    }
}