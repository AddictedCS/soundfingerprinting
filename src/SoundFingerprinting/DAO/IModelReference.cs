namespace SoundFingerprinting.DAO
{
    using ProtoBuf;

    [ProtoContract, ProtoInclude(100, typeof(ModelReference<int>)), ProtoInclude(101, typeof(ModelReference<ulong>))]
    public interface IModelReference
    {
        object Id { get; }
    }

    public interface IModelReference<out T> : IModelReference
    {
        new T Id { get; }
    }
}
