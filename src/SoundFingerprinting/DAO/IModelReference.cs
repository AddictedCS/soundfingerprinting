﻿namespace SoundFingerprinting.DAO
{
    using ProtoBuf;

    [ProtoContract, ProtoInclude(100, typeof(ModelReference<int>)), 
        ProtoInclude(101, typeof(ModelReference<ulong>)), 
        ProtoInclude(102, typeof(ModelReference<uint>)),
        ProtoInclude(103, typeof(CompoundModelReference<string>)),
        ProtoInclude(104, typeof(CompoundModelReference<int>))]
    
    public interface IModelReference
    {
        T Get<T>();
    }
}
