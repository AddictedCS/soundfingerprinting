namespace Soundfingerprinting.NeuralHashing.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    ///   Util class for generating and searching binary code pools.
    ///   Can find minimal L2norms across pools
    ///   Due to the fact that the pool can be very large, I/O operations should be implemented
    /// </summary>
    public static class BinaryOutputUtil
    {
        private static byte[][] _generatedBinaryCodesPool;
        private static int _generatedSize;
        private static bool _generated;

        #region BinaryCodesGeneration

        /// <summary>
        ///   Gets a list of binary codes
        /// </summary>
        /// <param name = "size">Size of output length. 2^n binary codes will be generated</param>
        /// <returns>2^size binary codes</returns>
        public static byte[][] GetAllBinaryCodes(int size)
        {
            if (size == _generatedSize && _generated)
                return _generatedBinaryCodesPool;
            int numberOfCombinations = (int) Math.Pow(2, size);
            _generatedBinaryCodesPool = new byte[numberOfCombinations][];

            for (int i = 0; i < numberOfCombinations; i++)
            {
                BitArray b = new BitArray(BitConverter.GetBytes(i));
                byte[] result = new byte[size];
                for (int j = 0; j < size; j++)
                    result[j] = b[j] ? (byte) 1 : (byte) 0;
                _generatedBinaryCodesPool[i] = result;
            }
            _generated = true;
            _generatedSize = size;
            return _generatedBinaryCodesPool;
        }

        /// <summary>
        ///   Get generated binary codes. If no codes has been previously generated, 2^10 binary codes will be returned.
        /// </summary>
        /// <returns>2^10 binary codes or previously generated</returns>
        public static byte[][] GetAllBinaryCodes()
        {
            //Default value for generated size
            _generatedSize = 10;
            return GetAllBinaryCodes(_generatedSize);
        }

        #endregion

        #region NormOperations

        /// <summary>
        ///   Searches for Binary Code - Track Code pair that have the minimal L2 Norm across
        ///   Pools of BinaryCode
        /// </summary>
        /// <param name = "binaryCodesPool">A pool with binary codes</param>
        /// <param name = "trackCodes">Pool with actual network outputs(float)</param>
        /// <returns>A pair of keys that correspond to the result of the search</returns>
        public static List<Tuple<int, int>> FindMinL2Norm(double[][] binaryCodesPool, double[][] trackCodes)
        {
            List<Tuple<float, ushort, ushort>> mainTupple = new List<Tuple<float, UInt16, UInt16>>(); /*Norm Value, Binary Code, Track Code*/
            /*Iterate through the collection in order to calculate the norms*/
            for (int i = 0, c = binaryCodesPool.GetLength(0); i < c; i++)
            {
                for (int j = 0, t = trackCodes.GetLength(0); j < t; j++)
                {
                    /*Calculating L2 Norm from vectors' subtraction. Skipping the usage of methods in order to increase performance*/
                    double sum = 0;
                    for (int k = 0, n = binaryCodesPool[i].Length; k < n; k++)
                    {
                        double subtraction = binaryCodesPool[i][k] - trackCodes[j][k];
                        sum += subtraction*subtraction;
                    }
                    float normValue = (float) Math.Sqrt(sum);
                    mainTupple.Add(new Tuple<float, UInt16, UInt16>(normValue, (UInt16) i, (UInt16) j));
                }
            }

            IOrderedEnumerable<Tuple<float, ushort, ushort>> ordered = mainTupple.OrderBy((tupple) => tupple.Item1); /*Sort the tupple descending by the L2Norm*/
            List<int> selectedBinaryCodes = new List<int>();
            List<int> selectedTracks = new List<int>();
            List<Tuple<int, int>> endResult = new List<Tuple<int, int>>();
            foreach (Tuple<float, ushort, ushort> item in ordered)
            {
                if (!selectedBinaryCodes.Contains(item.Item2))
                    if (!selectedTracks.Contains(item.Item3))
                    {
                        endResult.Add(new Tuple<int, int>(item.Item2, item.Item3));
                        selectedBinaryCodes.Add(item.Item2);
                        selectedTracks.Add(item.Item3);
                    }
            }

            return endResult;
        }

        /// <summary>
        ///   Subtracts two vectors, one being byte and double the other
        /// </summary>
        /// <param name = "firstVec">First vector of byte type</param>
        /// <param name = "secondVec">Second vector of double type</param>
        /// <returns>Resulting vector</returns>
        public static float[] VectorSubtraction(float[] firstVec, float[] secondVec)
        {
            if (firstVec.Length != secondVec.Length)
                throw new ArgumentException("Vectors must have the same length");

            float[] res = new float[firstVec.Length];
            for (int i = 0, l = res.Length; i < l; i++)
                res[i] = firstVec[i] - secondVec[i];
            return res;
        }

        /// <summary>
        ///   Computes the L2 norm of a vector
        /// </summary>
        /// <param name = "vector">Vector to compute on</param>
        /// <returns>The computed norm</returns>
        public static float L2Norm(float[] vector)
        {
            double res = 0;
            for (int i = 0, l = vector.Length; i < l; i++)
                res += vector[i]*vector[i];

            return (float) Math.Sqrt(res);
        }

        #endregion

        #region I/O Operations

        /// <summary>
        ///   Save Binary Codes in file
        /// </summary>
        /// <param name = "filename">Filename</param>
        public static void SaveBinaryCodes(string filename)
        {
            if (_generated)
                SaveBinaryCodes(filename, _generatedBinaryCodesPool);
            else
                throw new InvalidOperationException("Binary pool not generated");
        }

        /// <summary>
        ///   Save binary codes in file
        /// </summary>
        /// <param name = "filename">Filename</param>
        /// <param name = "binaryCodesPool">Binary codes to save</param>
        public static void SaveBinaryCodes(string filename, byte[][] binaryCodesPool)
        {
            FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            IFormatter formatter = new BinaryFormatter();

            try
            {
                formatter.Serialize(stream, binaryCodesPool);
            }
            finally
            {
                stream.Dispose();
            }
        }

        /// <summary>
        ///   Load binary codes
        /// </summary>
        /// <param name = "filename">Filename to load from</param>
        /// <returns>List of loaded binary codes</returns>
        public static byte[][] LoadBinaryCodes(string filename)
        {
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[][] result = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                result = (byte[][]) formatter.Deserialize(stream);
            }
            finally
            {
                stream.Dispose();
            }

            return result;
        }

        #endregion
    }
}