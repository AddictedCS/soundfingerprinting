namespace SoundFingerprinting.Dao.SQL
{
    using System.Collections.Generic;
    using System.Data;

    using SoundFingerprinting.Dao.Conditions;
    using SoundFingerprinting.Infrastructure;

    public class CachedModelBinderFactory : IModelBinderFactory
    {
        private readonly IModelBinderFactory modelBinderFactory;
        private readonly Dictionary<string, object> cache = new Dictionary<string, object>();

        public CachedModelBinderFactory()
            : this(DependencyResolver.Current.Get<IModelBinderFactory>())
        {
        }

        public CachedModelBinderFactory(IModelBinderFactory modelBinderFactory)
        {
            this.modelBinderFactory = modelBinderFactory;
        }

        public IModelBinder<TModel> Create<TModel>(params ICondition<TModel>[] conditions) where TModel : new()
        {
            if (cache.ContainsKey(typeof(TModel).FullName))
            {
                return (IModelBinder<TModel>)cache[typeof(TModel).FullName];
            }
            
            var modelBinder = modelBinderFactory.Create(conditions);
            cache[typeof(TModel).FullName] = modelBinder;
            return modelBinder;
        }

        public DbType GetParameterType<T>()
        {
            return modelBinderFactory.GetParameterType<T>();
        }
    }
}