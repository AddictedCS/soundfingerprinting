using System;

namespace SoundFingerprinting.Audio
{
    internal class LowPassFilter : ILowPassFilter
    {
        /*
         * Octave
         * Fs = 44100;
         * L = 31;
         * fc = 5512.5;
         * hsupp = (-(L-1)/2:(L-1)/2);
         * hideal = (fc/Fs)*sinc(fc*hsupp/Fs);
         * h = hamming(L)' .* hideal;
         * 
         * All filters are interpolating around 32 points
         */

        /*Low pass from 44100 to 5512*/
        private static float[] lp_filter44 =
        {
            -6.4966e-04f, -1.4478e-03f, -2.7094e-03f, -4.4524e-03f, -6.2078e-03f, -6.9775e-03f, -5.3848e-03f,
            2.3970e-18f, 1.0234e-02f, 2.5590e-02f, 4.5288e-02f, 6.7466e-02f, 8.9415e-02f, 1.0806e-01f, 1.2059e-01f,
            1.2500e-01f, 1.2059e-01f, 1.0806e-01f, 8.9415e-02f, 6.7466e-02f, 4.5288e-02f, 2.5590e-02f, 1.0234e-02f,
            2.3970e-18f, -5.3848e-03f, -6.9775e-03f, -6.2078e-03f, -4.4524e-03f, -2.7094e-03f, -1.4478e-03f,
            -6.4966e-04f
        };

        /*Low pass from 22050 to 5512*/
        private static float[] lp_filter22 =
        {
            -1.2004e-03f, -2.0475e-03f, -2.0737e-03f, 1.6358e-18f, 4.7512e-03f, 9.8676e-03f, 9.9498e-03f, -4.7939e-18f,
            -1.8909e-02f, -3.6189e-02f, -3.4662e-02f, 8.2622e-18f, 6.8435e-02f, 1.5283e-01f, 2.2282e-01f, 2.5000e-01f,
            2.2282e-01f, 1.5283e-01f, 6.8435e-02f, 8.2622e-18f, -3.4662e-02f, -3.6189e-02f, -1.8909e-02f, -4.7939e-18f,
            9.9498e-03f, 9.8676e-03f, 4.7512e-03f, 1.6358e-18f, -2.0737e-03f, -2.0475e-03f, -1.2004e-03f
        };

        /*Low pass from 11025 to 5512*/
        private static float[] lp_filter11 =
        {
            -1.6977e-03f, 1.7552e-18f, 2.9326e-03f, -3.2716e-18f, -6.7192e-03f, 6.0422e-18f, 1.4071e-02f, -9.5879e-18f,
            -2.6742e-02f, 1.3296e-17f, 4.9020e-02f, -1.6524e-17f, -9.6782e-02f, 1.8716e-17f, 3.1511e-01f, 5.0000e-01f,
            3.1511e-01f, 1.8716e-17f, -9.6782e-02f, -1.6524e-17f, 4.9020e-02f, 1.3296e-17f, -2.6742e-02f, -9.5879e-18f,
            1.4071e-02f, 6.0422e-18f, -6.7192e-03f, -3.2716e-18f, 2.9326e-03f, 1.7552e-18f, -1.6977e-03f
        };

        public float[] FilterAndDownsample(float[] samples, int sourceSampleRate, int targetSampleRate)
        {
            if (targetSampleRate != 5512)
                throw new ArgumentException($"Target sample {targetSampleRate} rate not supported!");

            switch (sourceSampleRate)
            {
                case 44100:
                    return Resample(samples, samples.Length / 8 - 31, 8, lp_filter44);
                case 22050:
                    return Resample(samples, samples.Length / 4 - 31, 4, lp_filter22);
                case 11025:
                    return Resample(samples, samples.Length / 2 - 31, 2, lp_filter11);
                case 5512:
                    return samples;
            }

            throw new ArgumentException($"Not supported sample rate {sourceSampleRate}");
        }

        private float[] Resample(float[] samples, int newSamplesCount, int mult, float[] filter)
        {
            float[] resampled = new float[newSamplesCount];
            for (int i = 0; i < newSamplesCount; i++)
            {
                resampled[i] = Convolve(samples, mult * i, filter, filter.Length);
            }
            return resampled;
        }

        private float Convolve(float[] start, int startIndex, float[] filter, int flen)
        {
            float sum = 0;
            for (int i = 0; i < flen; i++)
            {
                sum += start[startIndex + i] * filter[i];
            }

            return sum;
        }
    }

    internal interface ILowPassFilter
    {
        float[] FilterAndDownsample(float[] samples, int sourceSampleRate, int targetSampleRate);
    }
}
