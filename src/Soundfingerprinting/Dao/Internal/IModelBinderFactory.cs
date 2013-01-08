namespace Soundfingerprinting.Dao.Internal
{
    using System.Data;

    using Soundfingerprinting.Dao.Conditions;

    internal interface IModelBinderFactory
    {
        IModelBinder<TModel> Create<TModel>(params ICondition<TModel>[] conditions) where TModel : new();

        DbType GetParameterType<T>() where T : struct;
    }
}