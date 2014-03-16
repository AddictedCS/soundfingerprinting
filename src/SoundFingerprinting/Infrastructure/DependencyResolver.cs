namespace SoundFingerprinting.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Ninject;
    using Ninject.Parameters;

    internal static class DependencyResolver
    {
        private static IDependencyResolver dependencyResolver;

        public static IDependencyResolver Current
        {
            get
            {
                return dependencyResolver ?? (dependencyResolver = new DefaultDependencyResolver());
            }

            set
            {
                dependencyResolver = value;
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

            public TInterface Get<TInterface>(IEnumerable<ConstructorArgument> constructorArguments)
            {
                var arguments = constructorArguments.Select(constructorArgument => (IParameter)new Ninject.Parameters.ConstructorArgument(constructorArgument.Name, constructorArgument.Instance)).ToList();
                return kernel.Get<TInterface>(arguments.ToArray());
            }

            public void Bind<TInterface, TImplementation>() where TImplementation : TInterface
            {
                kernel.Rebind<TInterface>().To<TImplementation>();
            }

            public void BindAsSingleton<TInterface, TImplementation>() where TImplementation : TInterface
            {
                kernel.Rebind<TInterface>().To<TImplementation>().InSingletonScope();
            }

            public void Bind<TInterface, TImplementation>(TImplementation constant) where TImplementation : TInterface
            {
                kernel.Rebind<TInterface>().ToConstant(constant);
            }

            public void BindAsSingleton<TInterface, TImplementation>(TImplementation constant) where TImplementation : TInterface
            {
                kernel.Rebind<TInterface>().To<TImplementation>().InSingletonScope();
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
