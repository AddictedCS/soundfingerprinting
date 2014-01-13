namespace SoundFingerprinting.Data
{
    internal class ModelReference<T> : IModelReference
    {
        public ModelReference(T id)
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
