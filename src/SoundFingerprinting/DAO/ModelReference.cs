namespace SoundFingerprinting.DAO
{
    using System;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class ModelReference<T> : IModelReference<T>
    {
        public ModelReference()
        {
        }

        public ModelReference(T id)
        {
            Id = id;
        }

        [ProtoMember(1)]
        public T Id { get; private set; }

        object IModelReference.Id
        {
            get
            {
                return Id;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ModelReference<T>))
            {
                return false;
            }

            return Id.Equals(((ModelReference<T>)obj).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
