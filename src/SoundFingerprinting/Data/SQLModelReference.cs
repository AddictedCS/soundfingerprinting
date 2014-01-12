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

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is SQLModelReference<T>))
            {
                return false;
            }

            return Id.Equals(((SQLModelReference<T>)obj).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
