namespace Soundfingerprinting.Hashing.MinHash
{
    using Soundfingerprinting.Dao;
    using Soundfingerprinting.Infrastructure;

    public class DatabasePermutations : IPermutations
    {
        private readonly IModelService modelService;

        public DatabasePermutations()
            : this(DependencyResolver.Current.Get<IModelService>())
        {
        }

        public DatabasePermutations(IModelService modelService)
        {
            this.modelService = modelService;
        }

        public int[][] GetPermutations()
        {
            return ReadPermutations();
        }

        protected int[][] ReadPermutations()
        {
            return modelService.ReadPermutationsForLSHAlgorithm();
        }
    }
}