namespace Soundfingerprinting.Fingerprinting.Wavelets
{
    using System;
    using System.Collections.Generic;

    using Soundfingerprinting.Audio.Strides;

    //public interface IWaveletService
    //{

    //}

    //public class SpectrumService
    //{
    //    public List<float[][]> CutLogarithmizedSpectrum(
    //        float[][] spectrum, IStride stride, int fingerprintLength, int overlap)
    //    {
    //        int start = stride.FirstStrideSize / overlap;
    //        int logarithmicBins = spectrum[0].Length;
    //        List<float[][]> spectralImages = new List<float[][]>();

    //        int width = spectrum.GetLength(0);
    //        float[][] spectralImage = AllocateMemoryForFingerprintImage(fingerprintLength, logarithmicBins);
    //        while (start + fingerprintLength < width)
    //        {
    //            for (int i = 0; i < fingerprintLength; i++)
    //            {
    //                Array.Copy(spectrum[start + i], spectralImage[i], logarithmicBins);
    //            }

    //            start += fingerprintLength + (stride.StrideSize / overlap);
    //            spectralImages.Add(spectralImage);
    //        }

    //        return spectralImages;
    //    }

    //    private float[][] AllocateMemoryForFingerprintImage(int fingerprintLength, int logBins)
    //    {
    //        float[][] frames = new float[fingerprintLength][];
    //        for (int i = 0; i < fingerprintLength; i++)
    //        {
    //            frames[i] = new float[logBins];
    //        }

    //        return frames;
    //    }
    //}

    //public class WaveletService : IWaveletService
    //{
    //    private float[][][] WaveletTransform(float[][] spectrum)
    //    {

    //    }

    //    private List<bool[]> CreateFingerprintsFromSpectrum(
    //        float[][] spectrum, IStride stride, int fingerprintLength, int overlap, int logBins, int topWavelets)
    //    {
    //        int start = stride.FirstStrideSize / overlap;
    //        List<bool[]> fingerprints = new List<bool[]>();

    //        int width = spectrum.GetLength(0);
    //        float[][] frames = AllocateMemoryForFingerprintImage(fingerprintLength, logBins);
    //        while (start + fingerprintLength < width)
    //        {
    //            for (int i = 0; i < fingerprintLength; i++)
    //            {
    //                Array.Copy(spectrum[start + i], frames[i], logBins);
    //            }

    //            start += fingerprintLength + (stride.StrideSize / overlap);
    //            waveletDecomposition.DecomposeImageInPlace(frames); /*Compute wavelets*/
    //            bool[] image = fingerprintDescriptor.ExtractTopWavelets(frames, topWavelets);
    //            fingerprints.Add(image);
    //        }

    //        return fingerprints;
    //    }
    //}


}
