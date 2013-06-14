namespace SoundFingerprinting.Dao.Internal
{
    using System;
    using System.Linq;

    internal class PermutationsDao : AbstractDao
    {
        private const string SpReadAllPermutations = "sp_ReadPermutations";
        private const string FieldPermutationsPermutation = "Permutation";

        public PermutationsDao(IDatabaseProviderFactory databaseProvider, IModelBinderFactory modelBinderFactory)
            : base(databaseProvider, modelBinderFactory)
        {
        }

        public int[][] ReadPermutationsForLSHAlgorithm()
        {
            return PrepareStoredProcedure(SpReadAllPermutations).Execute().AsList(
                reader =>
                    {
                        string permutation = reader.GetString(FieldPermutationsPermutation);
                        string[] elementsOfPermutation = permutation.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        int[] arrayOfPermutations = new int[elementsOfPermutation.Length];
                        for (int i = 0; i < elementsOfPermutation.Length; i++)
                        {
                            arrayOfPermutations[i] = Convert.ToInt32(elementsOfPermutation[i]);
                        }

                        return arrayOfPermutations;
                    }).ToArray();
        }
    }
}