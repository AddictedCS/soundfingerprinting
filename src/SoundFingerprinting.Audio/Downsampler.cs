using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundFingerprinting.Audio
{
    class Downsampler
    {
        /*
         * Octave
         * Fs = 44100;
         * L = 31;
         * fc = 5512.5;
         * hsupp = (-(L-1)/2:(L-1)/2);
         * hideal = (fc/Fs)*sinc(fc*hsupp/Fs);
         * h = hamming(L)' .* hideal;
         */

        /* Low pass filter for taking 44100Hz to 5512.5 Hz */
        private static float[] lp_filter44 = new float[]
                                      {
                                          -6.4966e-04f, -1.4478e-03f, -2.7094e-03f, -4.4524e-03f,
                                          -6.2078e-03f, -6.9775e-03f, -5.3848e-03f, 2.3970e-18f,
                                          1.0234e-02f, 2.5590e-02f, 4.5288e-02f, 6.7466e-02f, 8.9415e-02f,
                                          1.0806e-01f, 1.2059e-01f, 1.2500e-01f, 1.2059e-01f, 1.0806e-01f,
                                          8.9415e-02f, 6.7466e-02f, 4.5288e-02f, 2.5590e-02f, 1.0234e-02f,
                                          2.3970e-18f, -5.3848e-03f, -6.9775e-03f, -6.2078e-03f,
                                          -4.4524e-03f, -2.7094e-03f, -1.4478e-03f, -6.4966e-04f
                                      };

        public static float[] Downsample(float[] samples, int sourceSampleRate, int targetSampleRate)
        {
            int newnsamples = 0, mult = 0;

            if (sourceSampleRate == 44100)
            {
                newnsamples = samples.Length / 8 - 3;
                mult = 8;
            }
            else
            {
                // Handle other
            }

            float[] resampled = new float[newnsamples];


            for (int i = 0; i < newnsamples; i++)
            {
                resampled[i] = convpt(samples, mult * i, lp_filter44, lp_filter44.Length);
            }

            return resampled;
        }

        /* Convolve a sample with a filter at a point. */
        private static float convpt(float[] start, int startIndex, float[] filter, int flen)
        {
            float sum = 0;

            for (int i = 0; i < flen; i++)
            {
                sum += start[startIndex + i] * filter[i];
            }

            return sum;
        }
    }
}
