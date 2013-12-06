<<<<<<< HEAD
namespace SoundFingerprinting.Dao
{
    using System;

    using SoundFingerprinting.Dao.Internal;

    public interface IModelBinder<TModel>
    {
        void BindWriter(TModel model, IParameterBinder storedProcedure);

        TModel BindReader(IReader reader);

        TModel BindReader(IReader reader, Func<TModel> modelFactory);
    }
=======
namespace SoundFingerprinting.Dao
{
    using System;

    using SoundFingerprinting.Dao.Internal;

    public interface IModelBinder<TModel>
    {
        void BindWriter(TModel model, IParameterBinder storedProcedure);

        TModel BindReader(IReader reader);

        TModel BindReader(IReader reader, Func<TModel> modelFactory);
    }
>>>>>>> 29ad7f2255c9e65f055245321140987dbe9f1382
}