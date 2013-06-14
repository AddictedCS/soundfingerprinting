namespace SoundFingerprinting.Dao
{
    using System.Data;

    using SoundFingerprinting.Dao.Conditions;

    public interface IModelBinderFactory
    {
        IModelBinder<TModel> Create<TModel>(params ICondition<TModel>[] conditions) where TModel : new();

        DbType GetParameterType<T>() where T : struct;
    }
}