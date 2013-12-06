<<<<<<< HEAD
namespace SoundFingerprinting.Dao
{
    using System.Data;

    using SoundFingerprinting.Dao.Conditions;

    public interface IModelBinderFactory
    {
        IModelBinder<TModel> Create<TModel>(params ICondition<TModel>[] conditions) where TModel : new();

        DbType GetParameterType<T>() where T : struct;
    }
=======
namespace SoundFingerprinting.Dao
{
    using System.Data;

    using SoundFingerprinting.Dao.Conditions;

    public interface IModelBinderFactory
    {
        IModelBinder<TModel> Create<TModel>(params ICondition<TModel>[] conditions) where TModel : new();

        DbType GetParameterType<T>() where T : struct;
    }
>>>>>>> 29ad7f2255c9e65f055245321140987dbe9f1382
}