namespace Soundfingerprinting.FFT.FFTW
{
    using System;

    /// <summary>
    /// FFTW planner flags
    /// </summary>
    [Flags]
    internal enum InteropFFTWFlags : uint
    {
        /// <summary>
        /// Tells FFTW to find an optimized plan by actually computing several FFTs and measuring their execution time. 
        /// Depending on your machine, this can take some time (often a few seconds). Default (0x0). 
        /// </summary>
        Measure = 0,

        /// <summary>
        /// Specifies that an out-of-place transform is allowed to overwrite its 
        /// input array with arbitrary data; this can sometimes allow more efficient algorithms to be employed.
        /// </summary>
        DestroyInput = 1,

        /// <summary>
        /// Rarely used. Specifies that the algorithm may not impose any unusual alignment requirements on the input/output 
        /// arrays (i.e. no SIMD). This flag is normally not necessary, since the planner automatically detects 
        /// misaligned arrays. The only use for this flag is if you want to use the guru interface to execute a given 
        /// plan on a different array that may not be aligned like the original. 
        /// </summary>
        Unaligned = 2,

        /// <summary>
        /// Not used.
        /// </summary>
        ConserveMemory = 4,

        /// <summary>
        /// Like Patient, but considers an even wider range of algorithms, including many that we think are 
        /// unlikely to be fast, to produce the most optimal plan but with a substantially increased planning time. 
        /// </summary>
        Exhaustive = 8,

        /// <summary>
        /// Specifies that an out-of-place transform must not change its input array. 
        /// </summary>
        /// <remarks>
        /// This is ordinarily the default, 
        /// except for c2r and hc2r (i.e. complex-to-real) transforms for which DestroyInput is the default. 
        /// In the latter cases, passing PreserveInput will attempt to use algorithms that do not destroy the 
        /// input, at the expense of worse performance; for multi-dimensional c2r transforms, however, no 
        /// input-preserving algorithms are implemented and the planner will return null if one is requested.
        /// </remarks>
        PreserveInput = 16,

        /// <summary>
        /// Like Measure, but considers a wider range of algorithms and often produces a “more optimal” plan 
        /// (especially for large transforms), but at the expense of several times longer planning time 
        /// (especially for large transforms).
        /// </summary>
        Patient = 32,

        /// <summary>
        /// Specifies that, instead of actual measurements of different algorithms, a simple heuristic is 
        /// used to pick a (probably sub-optimal) plan quickly. With this flag, the input/output arrays 
        /// are not overwritten during planning. 
        /// </summary>
        Estimate = 64
    }
}