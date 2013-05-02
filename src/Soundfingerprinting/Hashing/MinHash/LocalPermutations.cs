namespace Soundfingerprinting.Hashing.MinHash
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    /// <summary>
    ///   Class for reading local permutations from file
    /// </summary>
    public class LocalPermutations : IPermutations
    {
        /// <summary>
        ///   Path to permutations
        /// </summary>
        private readonly string _pathToPerms;

        /// <summary>
        ///   Separator between 2 consecutive indexes
        /// </summary>
        private readonly string _separator;

        /// <summary>
        ///   Permutations
        /// </summary>
        private int[][] _perms;

        /// <summary>
        ///   Local file permutation object
        /// </summary>
        /// <param name = "pathToPermutations">Path to file with permutations</param>
        /// <param name = "separator">Separator between 2 consecutive permutations</param>
        public LocalPermutations(string pathToPermutations, string separator)
        {
            this._pathToPerms = pathToPermutations;
            this._separator = separator;
        }

        #region IPermutations Members

        /// <summary>
        ///   Get permutations
        /// </summary>
        /// <returns>Permutations read from file</returns>
        public int[][] GetPermutations()
        {
            if (this._perms != null)
                return this._perms;
            List<int[]> result = new List<int[]>();
            using (StreamReader reader = new StreamReader(this._pathToPerms))
            {
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    if (line != null)
                    {
                        string[] ints = line.Split(new[] {this._separator}, StringSplitOptions.RemoveEmptyEntries);
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
            this._perms = result.ToArray();
            return this._perms;
        }

        #endregion
    }
}