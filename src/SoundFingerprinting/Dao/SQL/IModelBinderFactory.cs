namespace SoundFingerprinting.Dao.SQL
{
    using System.Data;

    using SoundFingerprinting.Dao.SQL.Conditions;

    internal interface IModelBinderFactory
    {
        IModelBinder<TModel> Create<TModel>(params ICondition<TModel>[] conditions) where TModel : new();

        DbType GetParameterType<T>();
    }
}