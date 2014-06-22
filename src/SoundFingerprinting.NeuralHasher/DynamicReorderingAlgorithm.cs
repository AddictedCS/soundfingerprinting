namespace SoundFingerprinting.NeuralHasher
{
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
    }
}
