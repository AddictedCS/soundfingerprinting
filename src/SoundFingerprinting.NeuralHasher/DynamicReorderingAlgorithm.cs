namespace SoundFingerprinting.NeuralHasher
{
    using System;
    using System.Collections.Generic;

    using Encog.ML;
    using Encog.ML.Data.Basic;

    internal class DynamicReorderingAlgorithm : IDynamicReorderingAlgorithm
    {
        public double[][] ComputeAm(IMLRegression network, List<double[][]> spectralImages, int outputCount)
        {
            int counter = 0;
            int trackCount = spectralImages.Count;
            double[][] am = new double[trackCount][]; 
            foreach (double[][] spectralImagesForTrack in spectralImages)
            {
                am[counter] = new double[outputCount];
                foreach (double[] snippet in spectralImagesForTrack)
                {
                    var actualOutput = network.Compute(new BasicMLData(snippet));
                    for (int binOutputIndex = 0; binOutputIndex < outputCount; binOutputIndex++)
                    {
                        am[counter][binOutputIndex] += actualOutput[binOutputIndex] / outputCount;
                    }
                }

                counter++;
            }

            return am;
        }

        public List<L2NormPair> CalculateL2NormPairs(double[][] binaryOutputs, double[][] am)
        {
            var normPairs = new List<L2NormPair>();

            for (int binaryOutputIndex = 0; binaryOutputIndex < binaryOutputs.Length; binaryOutputIndex++)
            {
                for (int snippetIndex = 0; snippetIndex < am.Length; snippetIndex++)
                {
                    double norm = L2Norm(binaryOutputs[binaryOutputIndex], am[snippetIndex]);
                    normPairs.Add(
                        new L2NormPair
                            {
                                L2Norm = norm, BinaryOutputIndex = binaryOutputIndex, SnippetIndex = snippetIndex 
                            });
                }
            }

            return normPairs;
        }

        private double L2Norm(double[] a, double[] b)
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
