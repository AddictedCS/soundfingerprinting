namespace SoundFingerprinting.DAO
{
    using ProtoBuf;

    [ProtoContract, ProtoInclude(200, typeof(IntModelReferenceProvider)),
     ProtoInclude(201, typeof(LongModelReferenceProvider)),
     ProtoInclude(202, typeof(UIntModelReferenceProvider)),
     ProtoInclude(203, typeof(CompoundModelReferenceProvider))]
    public interface IModelReferenceProvider
    {
        IModelReference Next();
    }
}