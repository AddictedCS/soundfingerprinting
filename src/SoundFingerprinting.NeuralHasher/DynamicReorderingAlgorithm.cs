namespace SoundFingerprinting.NeuralHasher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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

        public List<BestReorderingPair> FindBestReorderingPairs(List<L2NormPair> normPairs)
        {
            var orderedPairs = normPairs.OrderBy(pair => pair.L2Norm);

            var snippets = new HashSet<int>();
            var binaryCodes = new HashSet<int>();
            var bestPairs = new List<BestReorderingPair>();
            foreach (var orderedPair in orderedPairs)
            {
                if (!snippets.Contains(orderedPair.SnippetIndex) && !binaryCodes.Contains(orderedPair.BinaryOutputIndex))
                {
                    bestPairs.Add(new BestReorderingPair
                        {
                            BinaryOutputIndex = orderedPair.BinaryOutputIndex,
                            SnippetIndex = orderedPair.SnippetIndex
                        });
                    binaryCodes.Add(orderedPair.BinaryOutputIndex);
                    snippets.Add(orderedPair.SnippetIndex);
                }
            }

            return bestPairs;
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
