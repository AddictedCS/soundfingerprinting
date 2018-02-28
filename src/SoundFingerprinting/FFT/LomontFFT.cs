namespace SoundFingerprinting.FFT
{
    // Code to implement decently performing FFT for complex and real valued                                         
    // signals. See www.lomont.org for a derivation of the relevant algorithms                                       
    // from first principles. Copyright Chris Lomont 2010-2012.                                                      
    // This code and any ports are free for all to use for any reason as long                                        
    // as this header is left in place.                                                                              
    // Version 1.1, Sept 2011 
    using System;

    using SoundFingerprinting.Configuration;

    public unsafe class LomontFFT : IFFTService, IFFTServiceUnsafe
    {
        public LomontFFT()
        {
            A = 1;
            B = 1;
            var config = new DefaultSpectrogramConfig();
            Initialize(config.WdftSize);
        }

        public float[] FFTForward(float[] data, int startIndex, int length, float[] window)
        {
            float* toTransform = stackalloc float[length];
            for (int i = startIndex, j = 0; i < startIndex + length; ++i, ++j)
            {
                toTransform[j] = data[i];
            }

            Window(toTransform, window);
            RealFFT(toTransform, true, length);

            float[] transformed = new float[length];
            for (int i = 0; i < length; ++i)
            {
                transformed[i] = toTransform[i];
            }

            return transformed;
        }

        public void FFTForwardInPlace(float* data, int length)
        {
            RealFFT(data, true, length);
        }

        /// <summary>                                                                                            
        /// Compute the forward or inverse Fourier Transform of data, with data                                  
        /// containing complex valued data as alternating real and imaginary                                     
        /// parts. The length must be a power of 2. This method caches values                                    
        /// and should be slightly faster on than the FFT method for repeated uses.                              
        /// It is also slightly more accurate. Data is transformed in place.                                     
        /// </summary>                                                                                           
        /// <param name="data">The complex data stored as alternating real                                       
        /// and imaginary parts</param>                                                                          
        /// <param name="forward">true for a forward transform, false for                                        
        /// inverse transform</param>                                                                            
        private void TableFFT(float* data, bool forward, int length)
        {
            var n = length;
            // checks n is a power of 2 in 2's complement format                                                 
            if ((n & (n - 1)) != 0)
                throw new ArgumentException("data length " + n + " in FFT is not a power of 2");
            n /= 2;    // n is the number of samples                                                             

            Reverse(data, n); // bit index data reversal                                                         

            // do transform: so single point transforms, then doubles, etc.                                      
            float sign = forward ? B : -B;
            var mmax = 1;
            var tptr = 0;
            while (n > mmax)
            {
                var istep = 2 * mmax;
                for (var m = 0; m < istep; m += 2)
                {
                    float wr = cosTable[tptr];
                    float wi = sign * sinTable[tptr++];
                    for (var k = m; k < 2 * n; k += 2 * istep)
                    {
                        var j = k + istep;
                        var tempr = wr * data[j] - wi * data[j + 1];
                        var tempi = wi * data[j] + wr * data[j + 1];
                        data[j] = data[k] - tempr;
                        data[j + 1] = data[k + 1] - tempi;
                        data[k] = data[k] + tempr;
                        data[k + 1] = data[k + 1] + tempi;
                    }
                }
                mmax = istep;
            }


            // perform data scaling as needed                                                                    
            Scale(data, n, forward, length);
        }

        /// <summary>                                                                                            
        /// Compute the forward or inverse Fourier Transform of data, with                                       
        /// data containing real valued data only. The output is complex                                         
        /// valued after the first two entries, stored in alternating real                                       
        /// and imaginary parts. The first two returned entries are the real                                     
        /// parts of the first and last value from the conjugate symmetric                                       
        /// output, which are necessarily real. The length must be a power                                       
        /// of 2.                                                                                                
        /// </summary>                                                                                           
        /// <param name="data">The complex data stored as alternating real                                       
        /// and imaginary parts</param>                                                                          
        /// <param name="forward">true for a forward transform, false for                                        
        /// inverse transform</param>                                                                            
        internal void RealFFT(float* data, bool forward, int length)
        {
            var n = length; // # of real inputs, 1/2 the complex length                                     
            // checks n is a power of 2 in 2's complement format                                                 
            if ((n & (n - 1)) != 0)
                throw new ArgumentException("data length " + n + " in FFT is not a power of 2");

            float sign = -1.0f; // assume inverse FFT, this controls how algebra below works                        
            if (forward)
            { // do packed FFT. This can be changed to FFT to save memory                                        
                TableFFT(data, true, length);
                sign = 1.0f;
                // scaling - divide by scaling for N/2, then mult by scaling for N                               
                if (A != 1)
                {
                    var scale = (float)Math.Pow(2.0, (A - 1) / 2.0);
                    for (var i = 0; i < length; ++i)
                        data[i] *= scale;
                }
            }

            var theta = B * sign * 2 * Math.PI / n;
            var wpr = (float)Math.Cos(theta);
            var wpi = (float)Math.Sin(theta);
            var wjr = wpr;
            var wji = wpi;

            for (var j = 1; j <= n / 4; ++j)
            {
                var k = n / 2 - j;
                var tkr = data[2 * k];    // real and imaginary parts of t_k  = t_(n/2 - j)                      
                var tki = data[2 * k + 1];
                var tjr = data[2 * j];    // real and imaginary parts of t_j                                     
                var tji = data[2 * j + 1];

                var a = (tjr - tkr) * wji;
                var b = (tji + tki) * wjr;
                var c = (tjr - tkr) * wjr;
                var d = (tji + tki) * wji;
                var e = (tjr + tkr);
                var f = (tji - tki);

                // compute entry y[j]                                                                            
                data[2 * j] = 0.5f * (e + sign * (a + b));
                data[2 * j + 1] = 0.5f * (f + sign * (d - c));

                // compute entry y[k]                                                                            
                data[2 * k] = 0.5f * (e - sign * (b + a));
                data[2 * k + 1] = 0.5f * (sign * (d - c) - f);

                var temp = wjr;
                // todo - allow more accurate version here? make option?                                         
                wjr = wjr * wpr - wji * wpi;
                wji = temp * wpi + wji * wpr;
            }

            if (forward)
            {
                // compute final y0 and y_{N/2}, store in data[0], data[1]                                       
                var temp = data[0];
                data[0] += data[1];
                data[1] = temp - data[1];
            }
            else
            {
                var temp = data[0]; // unpack the y0 and y_{N/2}, then invert FFT                                
                data[0] = 0.5f * (temp + data[1]);
                data[1] = 0.5f * (temp - data[1]);
                // do packed inverse (table based) FFT. This can be changed to regular inverse FFT to save memory
                TableFFT(data, false, length);
                // scaling - divide by scaling for N, then mult by scaling for N/2                               
                //if (A != -1) // todo - off by factor of 2? this works, but something seems weird               
                {
                    var scale = (float)Math.Pow(2.0, -(A + 1) / 2.0) * 2;
                    for (var i = 0; i < length; ++i)
                        data[i] *= scale;
                }
            }
        }

        /// <summary>                                                                                            
        /// Determine how scaling works on the forward and inverse transforms.                                   
        /// For size N=2^n transforms, the forward transform gets divided by                                     
        /// N^((1-a)/2) and the inverse gets divided by N^((1+a)/2). Common                                      
        /// values for (A,B) are                                                                                 
        ///     ( 0, 1)  - default                                                                               
        ///     (-1, 1)  - data processing                                                                       
        ///     ( 1,-1)  - data processing                                                                     
        /// Usual values for A are 1, 0, or -1                                                                   
        /// </summary>                                                                                           
        private int A { get; set; }

        /// <summary>                                                                                            
        /// Determine how phase works on the forward and inverse transforms.                                     
        /// For size N=2^n transforms, the forward transform uses an                                             
        /// exp(B*2*pi/N) term and the inverse uses an exp(-B*2*pi/N) term.                                      
        /// Common values for (A,B) are                                                                          
        ///     ( 0, 1)  - default                                                                               
        ///     (-1, 1)  - data processing                                                                       
        ///     ( 1,-1)  - data processing                                                                     
        /// Abs(B) should be relatively prime to N.                                                              
        /// Setting B=-1 effectively corresponds to conjugating both input and                                   
        /// output data.                                                                                         
        /// Usual values for B are 1 or -1.                                                                      
        /// </summary>                                                                                           
        private int B { get; set; }

                #region Internals

        /// <summary>                                                                                            
        /// Scale data using n samples for forward and inverse transforms as needed                              
        /// </summary>                                                                                           
        /// <param name="data"></param>                                                                          
        /// <param name="n"></param>                                                                             
        /// <param name="forward"></param>                                                                       
        private void Scale(float* data, int n, bool forward, int length)
        {
            // forward scaling if needed                                                                         
            if ((forward) && (A != 1))
            {
                var scale = (float)Math.Pow(n, (A - 1) / 2.0);
                for (var i = 0; i < length; ++i)
                    data[i] *= scale;
            }

            // inverse scaling if needed                                                                         
            if ((!forward) && (A != -1))
            {
                var scale = (float)Math.Pow(n, -(A + 1) / 2.0);
                for (var i = 0; i < length; ++i)
                    data[i] *= scale;
            }
        }

        /// <summary>                                                                                            
        /// Call this with the size before using the TableFFT version                                            
        /// Fills in tables for speed. Done automatically in TableFFT                                            
        /// </summary>                                                                                           
        /// <param name="size">The size of the FFT in samples</param>                                            
        private void Initialize(int size)
        {
            // NOTE: if you port to non garbage collected languages                                              
            // like C# or Java be sure to free these correctly                                                   
            cosTable = new float[size];
            sinTable = new float[size];

            // forward pass                                                                                      
            var n = size;
            int mmax = 1, pos = 0;
            while (n > mmax)
            {
                var istep = 2 * mmax;
                var theta = Math.PI / mmax;
                float wr = 1, wi = 0;
                var wpi = (float) Math.Sin(theta);
                // compute in a slightly slower yet more accurate manner                                         
                var wpr = (float)System.Math.Sin(theta / 2);
                wpr = -2 * wpr * wpr;
                for (var m = 0; m < istep; m += 2)
                {
                    cosTable[pos] = wr;
                    sinTable[pos++] = wi;
                    var t = wr;
                    wr = wr * wpr - wi * wpi + wr;
                    wi = wi * wpr + t * wpi + wi;
                }
                mmax = istep;
            }
        }

        /// <summary>                                                                                            
        /// Swap data indices whenever index i has binary                                                        
        /// digits reversed from index j, where data is                                                          
        /// two doubles per index.                                                                               
        /// </summary>                                                                                           
        /// <param name="data"></param>                                                                          
        /// <param name="n"></param>                                                                             
        static void Reverse(float* data, int n)
        {
            // bit reverse the indices. This is exercise 5 in section                                            
            // 7.2.1.1 of Knuth's TAOCP the idea is a binary counter                                             
            // in k and one with bits reversed in j                                                              
            int j = 0, k = 0; // Knuth R1: initialize                                                            
            var top = n / 2;  // this is Knuth's 2^(n-1)                                                         
            while (true)
            {
                // Knuth R2: swap - swap j+1 and k+2^(n-1), 2 entries each                                       
                var t = data[j + 2];
                data[j + 2] = data[k + n];
                data[k + n] = t;
                t = data[j + 3];
                data[j + 3] = data[k + n + 1];
                data[k + n + 1] = t;
                if (j > k)
                { // swap two more                                                                               
                    // j and k                                                                                   
                    t = data[j];
                    data[j] = data[k];
                    data[k] = t;
                    t = data[j + 1];
                    data[j + 1] = data[k + 1];
                    data[k + 1] = t;
                    // j + top + 1 and k+top + 1                                                                 
                    t = data[j + n + 2];
                    data[j + n + 2] = data[k + n + 2];
                    data[k + n + 2] = t;
                    t = data[j + n + 3];
                    data[j + n + 3] = data[k + n + 3];
                    data[k + n + 3] = t;
                }
                // Knuth R3: advance k                                                                           
                k += 4;
                if (k >= n)
                    break;
                // Knuth R4: advance j                                                                           
                var h = top;
                while (j >= h)
                {
                    j -= h;
                    h /= 2;
                }
                j += h;
            } // bit reverse loop                                                                                
        }

        /// <summary>                                                                                            
        /// Pre-computed sine/cosine tables for speed                                                            
        /// </summary>                                                                                           
        private float[] cosTable;
        private float[] sinTable;

        #endregion

        private void Window(float* toTransform, float[] window)
        {
            for (int i = 0; i < window.Length; ++i)
            {
                toTransform[i] = toTransform[i] * window[i];
            }
        }
   }                                       
}
