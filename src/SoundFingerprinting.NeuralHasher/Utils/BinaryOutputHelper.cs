namespace SoundFingerprinting.NeuralHasher.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class BinaryOutputHelper : IBinaryOutputHelper
    {
        public byte[][] GetBinaryCodes(int size)
        {
            int numberOfCombinations = (int)Math.Pow(2, size);
            var binaryCodes = new byte[numberOfCombinations][];

            for (int i = 0; i < numberOfCombinations; i++)
            {
                BitArray b = new BitArray(BitConverter.GetBytes(i));
                byte[] result = new byte[size];
                for (int j = 0; j < size; j++)
                {
                    result[j] = b[j] ? (byte)1 : (byte)0;
                }

                binaryCodes[i] = result;
            }

            return binaryCodes;
        }

        public  List<Tuple<int, int>> FindMinL2Norm(double[][] binaryCodesPool, double[][] trackCodes)
        {
            List<Tuple<float, ushort, ushort>> mainTupple = new List<Tuple<float, ushort, ushort>>(); /*Norm Value, Binary Code, Track Code*/
            for (int i = 0;i<binaryCodesPool.GetLength(0); i++)
            {
                for (int j = 0; j < trackCodes.GetLength(0); j++)
                {
                    /*Calculating L2 Norm from vectors' subtraction. Skipping the usage of methods in order to increase performance*/
                    double sum = 0;
                    for (int k = 0, n = binaryCodesPool[i].Length; k < n; k++)
                    {
                        double subtraction = binaryCodesPool[i][k] - trackCodes[j][k];
                        sum += subtraction * subtraction;
                    }

                    float normValue = (float)Math.Sqrt(sum);
                    mainTupple.Add(new Tuple<float, ushort, ushort>(normValue, (ushort)i, (ushort)j));
                }
            }

            IOrderedEnumerable<Tuple<float, ushort, ushort>> ordered = mainTupple.OrderBy(tupple => tupple.Item1); /*Sort the tupple descending by the L2Norm*/
            List<int> selectedBinaryCodes = new List<int>();
            List<int> selectedTracks = new List<int>();
            List<Tuple<int, int>> endResult = new List<Tuple<int, int>>();
            foreach (Tuple<float, ushort, ushort> item in ordered)
            {
                if (!selectedBinaryCodes.Contains(item.Item2))
                {
                    if (!selectedTracks.Contains(item.Item3))
                    {
                        endResult.Add(new Tuple<int, int>(item.Item2, item.Item3));
                        selectedBinaryCodes.Add(item.Item2);
                        selectedTracks.Add(item.Item3);
                    }
                }
            }

            return endResult;
        }
    }
}