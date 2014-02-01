namespace SoundFingerprinting.Math
{
    using System;

    public static class HashingUtils
    {
        public static int CalculateHammingDistance(bool[] a, bool[] b)
        {
            int distance = 0;
            for (int i = 0, n = a.Length; i < n; i++)
            {
                if (a[i] != b[i])
                {
                    distance++;
                }
            }

            return distance;
        }

        public static int CalculateHammingDistance(byte[] a, byte[] b)
        {
            int distance = 0;
            for (int i = 0, n = a.Length; i < n; i++)
            {
                if (a[i] != b[i])
                {
                    distance++;
                }
            }

            return distance;
        }

        public static int CalculateHammingSimilarity(byte[] a, byte[] b)
        {
            return a.Length - CalculateHammingDistance(a, b);
        }

        public static int CalculateHammingDistance(long a, long b)
        {
            long dist = 0, val = a ^ b;

            // Count the number of set bits
            while (val != 0)
            {
                ++dist;
                val &= val - 1;
            }

            return (int)dist;
        }

        /// <summary>
        ///   Calculate similarity between 2 fingerprints.
        /// </summary>
        /// <param name = "x">Signature x</param>
        /// <param name = "y">Signature y</param>
        /// <returns>Jacquard similarity between array X and array Y</returns>
        /// <remarks>
        ///   Similarity defined as  (A intersection B)/(A union B)
        ///   for types of columns a (1,1), b(1,0), c(0,1) and d(0,0), it will be equal to
        ///   Sim(x,y) = a/(a+b+c)
        ///   +1 = 10
        ///   -1 = 01
        ///   0 = 00
        /// </remarks>
        public static double CalculateJaqSimilarity(bool[] x, bool[] y)
        {
            int a = 0, b = 0;
            for (int i = 0, n = x.Length; i < n; i++)
            {
                if (x[i] && y[i]) 
                {
                    // 1 1
                    a++;
                }
                else if ((x[i] && !y[i]) || (!x[i] && y[i])) 
                {
                    // 1 0 || 0 1 
                    b++;
                }
            }

            if (a + b == 0)
            {
                return 0;
            }

            return (double)a / (a + b);
        }

        /// <summary>
        ///   Calculates Jacquard similarity between 2 buckets
        /// </summary>
        /// <param name = "firstBucket">First bucket</param>
        /// <param name = "secondBucket">Second bucket</param>
        /// <returns>Jacquard similarity between bucket A and bucket B</returns>
        public static double CalculateSimilarity(long firstBucket, long secondBucket)
        {
            byte[] aarray = BitConverter.GetBytes(firstBucket);
            byte[] barray = BitConverter.GetBytes(secondBucket);
            int count = aarray.Length;
            int a = 0, b = 0;
            for (int i = 0; i < count; i++)
            {
                if (aarray[i] == barray[i] && aarray[i] != 0)
                {
                    a++;
                }
                else if (aarray[i] == barray[i])
                {
                    // no op
                }
                else
                {
                    b++;
                }
            }

            if (a + b == 0)
            {
                return 0;
            }

            return (double)a / (a + b);
        }
    }
}
