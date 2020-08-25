namespace SoundFingerprinting.DAO
{
    using System;
    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class ModelReference<T> : IModelReference
    {
        public ModelReference(T id)
        {
            Id = id;
        }

        private ModelReference()
        {
            // left for proto-buf
        }

        public static ModelReference<T> Null { get; } = new ModelReference<T>(default);

        [ProtoMember(1)]
        public T Id { get; }
        
        public TOut Get<TOut>()
        {
            if (Id is TOut get)
            {
                return get;
            }

            throw new InvalidCastException($"{typeof(T)} cannot be converted to {typeof(TOut)}");
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ModelReference<T> @object))
            {
                return false;
            }

            return Id.Equals(@object.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"ModelReference {nameof(Id)}: {Id}";
        }
    }
}
