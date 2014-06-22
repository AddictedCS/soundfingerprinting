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

        public List<Tuple<int, int>> FindMinL2Norm(double[][] binaryOutputs, double[][] am)
        {
            /*Norm Value, Binary Code, Track Code*/
            List<Tuple<double, int, int>> mainTupple = new List<Tuple<double, int, int>>();
            for (int i = 0; i < binaryOutputs.Length; i++)
            {
                for (int j = 0; j < am.Length; j++)
                {
                    double norm = L2Norm(binaryOutputs[i], am[j]);
                    mainTupple.Add(new Tuple<double, int, int>(norm, i, j));
                }
            }

            IOrderedEnumerable<Tuple<double, int, int>> ordered = mainTupple.OrderBy(tupple => tupple.Item1); /*Sort the tupple descending by the L2Norm*/
            List<int> selectedBinaryCodes = new List<int>();
            List<int> selectedTracks = new List<int>();
            List<Tuple<int, int>> endResult = new List<Tuple<int, int>>();
            foreach (Tuple<double, int, int> item in ordered)
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

        public double L2Norm(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                double subtracted = a[i] - b[i];
                sum += subtracted * subtracted;
            }

            return Math.Sqrt(sum);
        }
    }
}