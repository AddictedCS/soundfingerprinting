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
}