namespace SoundFingerprinting.SQL.ORM
{
    using System.Data;

    using SoundFingerprinting.SQL.Conditions;

    internal interface IModelBinderFactory
    {
        IModelBinder<TModel> Create<TModel>(params ICondition<TModel>[] conditions) where TModel : new();

        DbType GetParameterType<T>();
    }
}