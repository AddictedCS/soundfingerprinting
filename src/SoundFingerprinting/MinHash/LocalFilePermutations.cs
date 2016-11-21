namespace SoundFingerprinting.MinHash
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    internal class LocalFilePermutations : IPermutations
    {
        private readonly string pathToPermutations;

        private readonly string separator;

        private int[][] permutations;
        
        public LocalFilePermutations(string pathToPermutations, string separator)
        {
            this.pathToPermutations = pathToPermutations;
            this.separator = separator;
        }

        public int[][] GetPermutations()
        {
            if (permutations != null)
            {
                return permutations;
            }

            List<int[]> result = new List<int[]>();
            using (StreamReader reader = new StreamReader(pathToPermutations))
            {
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    if (line != null)
                    {
                        string[] ints = line.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                        int[] permutation = new int[ints.Length];
                        int i = 0;
                        foreach (string item in ints)
                        {
                            permutation[i++] = Convert.ToInt32(item, CultureInfo.InvariantCulture);
                        }

                        result.Add(permutation);
                    }
                }
            }

            permutations = result.ToArray();
            return permutations;
        }
    }
}