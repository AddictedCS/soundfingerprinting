namespace SoundFingerprinting.NeuralHasher
{
    using System.Collections.Generic;

    using Encog.ML;

    internal interface IDynamicReorderingAlgorithm
    {
        double[][] ComputeAm(IMLRegression network, List<double[][]> spectralImages, int outputCount);
    }
}