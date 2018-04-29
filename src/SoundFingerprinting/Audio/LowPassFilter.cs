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

        private static float[] lp_filter49to48Khz =
        {
            0.0016977f, 0.0020362f, 0.0028684f, -0.0042343f, 0.0061380f, -0.0085451f, 0.0113830f, -0.0145443f,
            0.0178923f, -0.0212695f, 0.0245076f, -0.0274382f, 0.0299043f, -0.0317711f, 0.0329347f, 0.9666700f,
            0.0329347f, -0.0317711f, 0.0299043f, -0.0274382f, 0.0245076f, -0.0212695f, 0.0178923f, -0.0145443f,
            0.0113830f, -0.0085451f, 0.0061380f, -0.0042343f, 0.0028684f, -0.0020362f, 0.0016977f
        };

        private static float[] lp_filter49Khz =
        {
            -1.4702e-03f, -2.0164e-03f, -2.8880e-03f, -3.8559e-03f, -4.3190e-03f, -3.3748e-03f, 1.7805e-07f,
            6.6945e-03f, 1.7190e-02f, 3.1341e-02f, 4.8275e-02f, 6.6441e-02f, 8.3815e-02f, 9.8234e-02f, 1.0777e-01f,
            1.1111e-01f, 1.0777e-01f, 9.8234e-02f, 8.3815e-02f, 6.6441e-02f, 4.8275e-02f, 3.1341e-02f, 1.7190e-02f,
            6.6945e-03f, 1.7805e-07f, -3.3748e-03f, -4.3190e-03f, -3.8559e-03f, -2.8880e-03f, -2.0164e-03f, -1.4702e-03f
        };

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
                case 48000:
                    return ResampleNonIntegerFactor(samples, 29, 9, lp_filter49Khz);
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

        private float[] ResampleNonIntegerFactor(float[] samples, int p, int q, float[] filter)
        {
            int zeros = samples.Length / p; // number of zeros to add

            float[] buffer = new float[samples.Length + zeros];

            for (int i = 0; i < zeros; ++i)
            {
                Buffer.BlockCopy(samples, i * p * sizeof(float), buffer, i * (p + 1) * sizeof(float), p * sizeof(float));
                var sum = Convolve(samples, (i + 1) * p - 16, lp_filter49to48Khz, lp_filter49to48Khz.Length);
                buffer[(i + 1) * (p + 1) - 1] = sum;
            }

            return Resample(buffer, buffer.Length / q - 31, q, filter);
        }

        private static float Avg(float[] samples, int p, int i)
        {
            float sum = 0f;
            for (int j = (i + 1) * p - 16; j < (i + 1) * p + 16 && j < samples.Length; ++j)
            {
                sum += samples[j];
            }

            return sum / 32;
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
