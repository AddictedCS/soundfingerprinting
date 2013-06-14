namespace SoundFingerprinting.Dao.Internal
{
    using System;

    internal class ModelBinder<TModel> : IModelBinder<TModel>
        where TModel : new()
    {
        private readonly Action<IReader, TModel> readerAction;
        private readonly Action<IParameterBinder, TModel> writerAction;

        public ModelBinder(Action<IReader, TModel> readerAction, Action<IParameterBinder, TModel> writerAction)
        {
            this.readerAction = readerAction;
            this.writerAction = writerAction;
        }

        public void BindWriter(TModel model, IParameterBinder storedProcedure)
        {
            writerAction(storedProcedure, model);
        }

        public TModel BindReader(IReader reader)
        {
            return BindReader(reader, () => new TModel());
        }

        public TModel BindReader(IReader reader, Func<TModel> modelFactory)
        {
            TModel model = modelFactory();
            readerAction(reader, model);
            return model;
        }
    }
}