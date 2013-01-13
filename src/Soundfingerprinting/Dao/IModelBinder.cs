namespace Soundfingerprinting.Dao
{
    using System;

    using Soundfingerprinting.Dao.Internal;

    public interface IModelBinder<TModel>
    {
        void BindWriter(TModel model, IParameterBinder storedProcedure);

        TModel BindReader(IReader reader);

        TModel BindReader(IReader reader, Func<TModel> modelFactory);
    }
}