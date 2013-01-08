namespace Soundfingerprinting.Dao.Internal
{
    using System.Collections.Generic;
    using System.Data;

    using Soundfingerprinting.Dao.Conditions;

    internal class CachedModelBinderFactory : IModelBinderFactory
    {
        private readonly IModelBinderFactory modelBinderFactory;
        private readonly Dictionary<string, object> cache;

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

        public DbType GetParameterType<T>() where T : struct
        {
            return modelBinderFactory.GetParameterType<T>();
        }
    }
}