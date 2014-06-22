namespace SoundFingerprinting.NeuralHasher
{
    using System.Collections.Generic;

    using Encog.ML;

    internal interface IDynamicReorderingAlgorithm
    {
        double[][] ComputeAm(IMLRegression network, List<double[][]> spectralImages, int outputCount);

        List<L2NormPair> CalculateL2NormPairs(double[][] binaryOutputs, double[][] am);

        List<BestReorderingPair> FindBestReorderingPairs(List<L2NormPair> normPairs);
    }
}