namespace SoundFingerprinting.Data
{
    public interface IModelReference
    {
        object Id { get; }
    }

    public interface IModelReference<out T> : IModelReference
    {
        new T Id { get; }
    }
}
