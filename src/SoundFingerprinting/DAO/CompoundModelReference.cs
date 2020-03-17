namespace SoundFingerprinting.DAO
{
    using System;
    using ProtoBuf;

    [ProtoContract]
    public class CompoundModelReference<T> : IModelReference
    {
        [ProtoMember(1)]
        public T Prefix { get; }

        [ProtoMember(2)]
        public IModelReference Reference { get; }

        public object Id => Reference.Id;

        private CompoundModelReference()
        {
            // left for proto-buf
        }

        public CompoundModelReference(T prefix, IModelReference reference)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }
            Prefix = prefix;
            Reference = reference ?? throw new ArgumentNullException(nameof(reference));
        }

        public override bool Equals(object obj)
        {
            var other = obj as CompoundModelReference<T>;
            if (other == null)
            {
                return false;
            }
            return Prefix.Equals(other.Prefix)
                && Reference.Equals(other.Reference);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var code = 17;
                code = 31 * code + Prefix.GetHashCode();
                code = 31 * code + Reference.GetHashCode();
                return code;
            }
        }

        public override string ToString()
        {
            return $"CompoundModelReference {nameof(Prefix)}: {Prefix}, {nameof(Reference)}: {Reference}";
        }
    }
}