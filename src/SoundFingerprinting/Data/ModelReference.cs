namespace SoundFingerprinting.Data
{
    public class ModelReference<T> : IModelReference<T>
    {
        public ModelReference(T id)
        {
            Id = id;
        }

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
