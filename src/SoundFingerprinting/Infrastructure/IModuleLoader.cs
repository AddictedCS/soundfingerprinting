namespace SoundFingerprinting.Infrastructure
{
    using Ninject;

    public interface IModuleLoader
    {
        void LoadAssemblyBindings(IKernel kernel);
    }
}
