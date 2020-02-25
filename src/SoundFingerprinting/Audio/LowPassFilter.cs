﻿namespace SoundFingerprinting.Audio
{
    using System;

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

        private static readonly float[] LpFilter96KhzTo48Khz =
        {
            -1.6977e-03f, 1.7552e-18f, 2.9326e-03f, -3.2716e-18f, -6.7192e-03f, 6.0422e-18f, 1.4071e-02f, -9.5879e-18f, -2.6742e-02f, 1.3296e-17f, 4.9020e-02f,
            -1.6524e-17f, -9.6782e-02f, 1.8716e-17f, 3.1511e-01f, 5.0000e-01f, 3.1511e-01f, 1.8716e-17f, -9.6782e-02f, -1.6524e-17f, 4.9020e-02f, 1.3296e-17f,
            -2.6742e-02f, -9.5879e-18f, 1.4071e-02f, 6.0422e-18f, -6.7192e-03f, -3.2716e-18f, 2.9326e-03f, 1.7552e-18f, -1.6977e-03f
        };
        
        // L = 63, Fs = 72000
        private static readonly float[] LpFilter72KHz64 =
        {
            7.5736e-04f, 7.0190e-04f, 6.2634e-04f, 5.0156e-04f, 2.8968e-04f, -4.9681e-05f, -5.5166e-04f, -1.2376e-03f,
            -2.1067e-03f, -3.1292e-03f, -4.2414e-03f, -5.3443e-03f, -6.3058e-03f, -6.9665e-03f, -7.1501e-03f,
            -6.6760e-03f, -5.3745e-03f, -3.1034e-03f, 2.3628e-04f, 4.6867e-03f, 1.0222e-02f, 1.6745e-02f, 2.4081e-02f,
            3.1989e-02f, 4.0169e-02f, 4.8282e-02f, 5.5963e-02f, 6.2852e-02f, 6.8611e-02f, 7.2950e-02f, 7.5647e-02f,
            7.6563e-02f, 7.5647e-02f, 7.2950e-02f, 6.8611e-02f, 6.2852e-02f, 5.5963e-02f, 4.8282e-02f, 4.0169e-02f,
            3.1989e-02f, 2.4081e-02f, 1.6745e-02f, 1.0222e-02f, 4.6867e-03f, 2.3628e-04f, -3.1034e-03f, -5.3745e-03f,
            -6.6760e-03f, -7.1501e-03f, -6.9665e-03f, -6.3058e-03f, -5.3443e-03f, -4.2414e-03f, -3.1292e-03f,
            -2.1067e-03f - 1.2376e-03f, -5.5166e-04f, -4.9681e-05f, 2.8968e-04f, 5.0156e-04f, 6.2634e-04f, 7.0190e-04f,
            7.5736e-04f
        };

        // L = 127
        private static readonly float[] LpFilter336KHz128 =
        {
            -4.2580e-05f, -2.2325e-05f, -1.0539e-06f, 2.2162e-05f, 4.8302e-05f, 7.8386e-05f, 1.1347e-04f, 1.5463e-04f,
            2.0296e-04f, 2.5957e-04f, 3.2554e-04f, 4.0195e-04f, 4.8987e-04f, 5.9031e-04f, 7.0424e-04f, 8.3258e-04f,
            9.7618e-04f, 1.1358e-03f, 1.3122e-03f, 1.5059e-03f, 1.7174e-03f, 1.9470e-03f, 2.1952e-03f, 2.4620e-03f,
            2.7474e-03f, 3.0514e-03f, 3.3737e-03f, 3.7138e-03f, 4.0714e-03f, 4.4457e-03f, 4.8359e-03f, 5.2412e-03f,
            5.6604e-03f, 6.0925e-03f, 6.5360e-03f, 6.9896e-03f, 7.4517e-03f, 7.9207e-03f, 8.3950e-03f, 8.8726e-03f,
            9.3517e-03f, 9.8304e-03f, 1.0307e-02f, 1.0779e-02f, 1.1244e-02f, 1.1701e-02f, 1.2147e-02f, 1.2581e-02f,
            1.3000e-02f, 1.3403e-02f, 1.3787e-02f, 1.4151e-02f, 1.4492e-02f, 1.4811e-02f, 1.5103e-02f, 1.5369e-02f,
            1.5607e-02f, 1.5816e-02f, 1.5994e-02f, 1.6142e-02f, 1.6257e-02f, 1.6340e-02f, 1.6390e-02f, 1.6406e-02f,
            1.6390e-02f, 1.6340e-02f, 1.6257e-02f, 1.6142e-02f, 1.5994e-02f, 1.5816e-02f, 1.5607e-02f, 1.5369e-02f,
            1.5103e-02f, 1.4811e-02f, 1.4492e-02f, 1.4151e-02f, 1.3787e-02f, 1.3403e-02f, 1.3000e-02f, 1.2581e-02f,
            1.2147e-02f, 1.1701e-02f, 1.1244e-02f, 1.0779e-02f, 1.0307e-02f, 9.8304e-03f, 9.3517e-03f, 8.8726e-03f,
            8.3950e-03f, 7.9207e-03f, 7.4517e-03f, 6.9896e-03f, 6.5360e-03f, 6.0925e-03f, 5.6604e-03f, 5.2412e-03f,
            4.8359e-03f, 4.4457e-03f, 4.0714e-03f, 3.7138e-03f, 3.3737e-03f, 3.0514e-03f, 2.7474e-03f, 2.4620e-03f,
            2.1952e-03f, 1.9470e-03f, 1.7174e-03f, 1.5059e-03f, 1.3122e-03f, 1.1358e-03f, 9.7618e-04f, 8.3258e-04f,
            7.0424e-04f, 5.9031e-04f, 4.8987e-04f, 4.0195e-04f, 3.2554e-04f, 2.5957e-04f, 2.0296e-04f, 1.5463e-04f,
            1.1347e-04f, 7.8386e-05f, 4.8302e-05f, 2.2162e-05f, -1.0539e-06f, -2.2325e-05f, -4.2580e-05f
        };

        // Low pass from 44100 to 5512
        private static readonly float[] LpFilter44 =
        {
            -6.4966e-04f, -1.4478e-03f, -2.7094e-03f, -4.4524e-03f, -6.2078e-03f, -6.9775e-03f, -5.3848e-03f,
            2.3970e-18f, 1.0234e-02f, 2.5590e-02f, 4.5288e-02f, 6.7466e-02f, 8.9415e-02f, 1.0806e-01f, 1.2059e-01f,
            1.2500e-01f, 1.2059e-01f, 1.0806e-01f, 8.9415e-02f, 6.7466e-02f, 4.5288e-02f, 2.5590e-02f, 1.0234e-02f,
            2.3970e-18f, -5.3848e-03f, -6.9775e-03f, -6.2078e-03f, -4.4524e-03f, -2.7094e-03f, -1.4478e-03f,
            -6.4966e-04f
        };

        // Low pass from 22050 to 5512
        private static readonly float[] LpFilter22 =
        {
            -1.2004e-03f, -2.0475e-03f, -2.0737e-03f, 1.6358e-18f, 4.7512e-03f, 9.8676e-03f, 9.9498e-03f, -4.7939e-18f,
            -1.8909e-02f, -3.6189e-02f, -3.4662e-02f, 8.2622e-18f, 6.8435e-02f, 1.5283e-01f, 2.2282e-01f, 2.5000e-01f,
            2.2282e-01f, 1.5283e-01f, 6.8435e-02f, 8.2622e-18f, -3.4662e-02f, -3.6189e-02f, -1.8909e-02f, -4.7939e-18f,
            9.9498e-03f, 9.8676e-03f, 4.7512e-03f, 1.6358e-18f, -2.0737e-03f, -2.0475e-03f, -1.2004e-03f
        };

        // Low pass from 11025 to 5512
        private static readonly float[] LpFilter11 =
        {
            -1.6977e-03f, 1.7552e-18f, 2.9326e-03f, -3.2716e-18f, -6.7192e-03f, 6.0422e-18f, 1.4071e-02f, -9.5879e-18f,
            -2.6742e-02f, 1.3296e-17f, 4.9020e-02f, -1.6524e-17f, -9.6782e-02f, 1.8716e-17f, 3.1511e-01f, 5.0000e-01f,
            3.1511e-01f, 1.8716e-17f, -9.6782e-02f, -1.6524e-17f, 4.9020e-02f, 1.3296e-17f, -2.6742e-02f, -9.5879e-18f,
            1.4071e-02f, 6.0422e-18f, -6.7192e-03f, -3.2716e-18f, 2.9326e-03f, 1.7552e-18f, -1.6977e-03f
        };

        public float[] FilterAndDownsample(float[] samples, int sourceSampleRate, int targetSampleRate)
        {
            if (targetSampleRate != 5512)
            {
                throw new ArgumentException($"Target sample {targetSampleRate} rate not supported!");
            }

            switch (sourceSampleRate)
            {
                case 96000:
                    //96Khz -> 48Khz then to 5512Khz
                    var f48Khz =  Resample(samples, samples.Length / 2, 2, LpFilter96KhzTo48Khz);
                    return ResampleNonIntegerFactor(f48Khz, 7, 61, LpFilter336KHz128);
                case 48000:
                    // 48000 * 7 / 61 is almost 5512
                    return ResampleNonIntegerFactor(samples, 7 , 61, LpFilter336KHz128);
                case 44100:
                    return Resample(samples, samples.Length / 8, 8, LpFilter44);
                case 22050:
                    return Resample(samples, samples.Length / 4, 4, LpFilter22);
                case 16000:
                    // 16000 * 21 / 61 is almost 5512
                    return ResampleNonIntegerFactor(samples, 21, 61, LpFilter336KHz128);
                case 11025:
                    return Resample(samples, samples.Length / 2, 2, LpFilter11);
                case 8000:
                    // 8000 * 9 / 13 is almost 5512
                    return ResampleNonIntegerFactor(samples, 9, 13, LpFilter72KHz64);
                case 5512:
                    return samples;
            }

            throw new ArgumentException($"Not supported sample rate {sourceSampleRate}");
        }

        private float[] ResampleNonIntegerFactor(float[] samples, int p, int q, float[] filter)
        {
            float[] buffer = new float[samples.Length * p];
            for (int i = 0; i < buffer.Length; i += p)
            {
                buffer[i] = samples[i / p];
            }

            return Resample(buffer, buffer.Length / q - filter.Length, q, filter);
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

        private float Convolve(float[] buffer, int begin, float[] filter, int flen)
        {
            float sum = 0;
            for (int index = 0; index < flen && begin + index < buffer.Length; ++index)
            {
                sum += buffer[begin + index] * filter[index];
            }

            return sum;
        }
    }
}
