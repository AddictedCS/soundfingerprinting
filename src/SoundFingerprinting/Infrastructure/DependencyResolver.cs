namespace SoundFingerprinting.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Ninject;

    internal static class DependencyResolver
    {
        private static IDependencyResolver dependencyResolver;

        public static IDependencyResolver Current
        {
            get
            {
                return dependencyResolver ?? (dependencyResolver = new DefaultDependencyResolver());
            }
        }

        private sealed class DefaultDependencyResolver : IDependencyResolver, IDisposable
        {
            private readonly IKernel kernel;

            public DefaultDependencyResolver()
            {
                kernel = new StandardKernel();
                LoadAllAssemblyBindings();
            }

            ~DefaultDependencyResolver()
            {
                Dispose(false);
            }

            public T Get<T>()
            {
                return kernel.Get<T>();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool isDisposing)
            {
                if (isDisposing)
                {
                    kernel.Dispose();
                }
            }

            private void LoadAllAssemblyBindings()
            {
                const string MainAssemblyName = "SoundFingerprinting";
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies() 
                                                .Where(assembly => assembly.FullName.Contains(MainAssemblyName));

                foreach (var loadedAssembly in loadedAssemblies)
                {
                    var moduleLoaders = GetModuleLoaders(loadedAssembly);
                    foreach (var moduleLoader in moduleLoaders)
                    {
                        moduleLoader.LoadAssemblyBindings(kernel);
                    }
                }
            }

            private IEnumerable<IModuleLoader> GetModuleLoaders(Assembly loadedAssembly)
            {
                var moduleLoaders = from type in loadedAssembly.GetTypes()
                                    where type.GetInterfaces().Contains(typeof(IModuleLoader)) && type.GetConstructor(Type.EmptyTypes) != null
                                    select Activator.CreateInstance(type) as IModuleLoader;
                return moduleLoaders;
            }
        }
    }
}
