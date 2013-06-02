namespace Soundfingerprinting.Infrastructure
{
    using System;
    using System.Collections.Generic;

    public interface IDependencyResolver
    {
        object GetService(Type serviceType);

        IEnumerable<object> GetServices(Type serviceType);

        T Get<T>();
    }
}
