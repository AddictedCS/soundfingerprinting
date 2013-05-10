namespace Soundfingerprinting.SoundTools.DI
{
    using System;
    using System.Collections.Generic;

    using Ninject;
    using Ninject.Modules;
    using Ninject.Syntax;

    public class NinjectDependencyResolver : IDependencyResolver
    {
        private readonly IKernel kernel;

        public NinjectDependencyResolver()
        {
            kernel = new StandardKernel();
        }

        public NinjectDependencyResolver(INinjectSettings settings, params INinjectModule[] modules)
        {
            kernel = new StandardKernel(settings, modules);
        }

        public IKernel Kernel
        {
            get
            {
                return kernel;
            }
        }

        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }

        public IBindingToSyntax<T> Bind<T>()
        {
            return kernel.Bind<T>();
        }

        public IBindingToSyntax<T1, T2> Bind<T1, T2>()
        {
            return kernel.Bind<T1, T2>();
        }

        public IBindingToSyntax<T1, T2, T3> Bind<T1, T2, T3>()
        {
            return kernel.Bind<T1, T2, T3>();
        }
        
        public T Get<T>()
        {
            return kernel.Get<T>();
        }
    }
}