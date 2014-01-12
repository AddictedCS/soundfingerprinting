namespace SoundFingerprinting.Data
{
    internal class SQLModelReference<T> : IModelReference
    {
        public SQLModelReference(T id)
        {
            Id = id;
        }

        public T Id { get; private set; }

        public int HashCode
        {
            get
            {
                return Id.GetHashCode();
            }
        }
    }
}
