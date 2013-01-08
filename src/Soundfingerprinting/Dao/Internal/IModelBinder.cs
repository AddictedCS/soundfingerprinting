namespace Soundfingerprinting.Dao.Internal
{
    using System;

    internal interface IModelBinder<TModel>
    {
        void BindWriter(TModel model, IParameterBinder storedProcedure);

        TModel BindReader(IReader reader);

        TModel BindReader(IReader reader, Func<TModel> modelFactory);
    }
}