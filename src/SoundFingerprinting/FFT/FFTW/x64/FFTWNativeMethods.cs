namespace SoundFingerprinting.FFT.FFTW.X64
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Contains the Basic Interface FFTW functions for double-precision (double) operations
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1612:ElementParameterDocumentationMustMatchElementParameters", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1625:ElementDocumentationMustNotBeCopiedAndPasted", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1615:ElementReturnValueMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    internal class FFTWNativeMethods
    {
        /// <summary>
        /// Allocates FFTW-optimized unmanaged memory
        /// </summary>
        /// <param name="length">Amount to allocate, in bytes</param>
        /// <returns>Pointer to allocated memory</returns>
        [DllImport(@"x64\libfftw3-3.dll", EntryPoint = "fftw_malloc", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr malloc(int length);

        /// <summary>
        /// Deallocates memory allocated by FFTW malloc
        /// </summary>
        /// <param name="mem">Pointer to memory to release</param>
        [DllImport(@"x64\libfftw3-3.dll", EntryPoint = "fftw_free", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void free(IntPtr mem);

        /// <summary>
        /// Deallocates an FFTW plan and all associated resources
        /// </summary>
        /// <param name="plan">Pointer to the plan to release</param>
        [DllImport(@"x64\libfftw3-3.dll", EntryPoint = "fftw_destroy_plan", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void destroy_plan(IntPtr plan);

        /// <summary>
        /// Clears all memory used by FFTW, resets it to initial state. Does not replace destroy_plan and free
        /// </summary>
        /// <remarks>After calling fftw_cleanup, all existing plans become undefined, and you should not 
        /// attempt to execute them nor to destroy them. You can however create and execute/destroy new plans, 
        /// in which case FFTW starts accumulating wisdom information again. 
        /// fftw_cleanup does not deallocate your plans; you should still call fftw_destroy_plan for this purpose.</remarks>
        [DllImport(@"x64\libfftw3-3.dll", EntryPoint = "fftw_cleanup", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void cleanup();

        /// <summary>
        /// Sets the maximum time that can be used by the planner.
        /// </summary>
        /// <param name="seconds">Maximum time, in seconds.</param>
        /// <remarks>This function instructs FFTW to spend at most seconds seconds (approximately) in the planner. 
        /// If seconds == -1.0 (the default value), then planning time is unbounded. 
        /// Otherwise, FFTW plans with a progressively wider range of algorithms until the the given time limit is 
        /// reached or the given range of algorithms is explored, returning the best available plan. For example, 
        /// specifying InteropFFTWFlags.Patient first plans in Estimate mode, then in Measure mode, then finally (time 
        /// permitting) in Patient. If InteropFFTWFlags.Exhaustive is specified instead, the planner will further progress to 
        /// Exhaustive mode. 
        /// </remarks>
        [DllImport(@"x64\libfftw3-3.dll", EntryPoint = "fftw_set_timelimit", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_timelimit(double seconds);

        /// <summary>
        /// Executes an FFTW plan, provided that the input and output arrays still exist
        /// </summary>
        /// <param name="plan">Pointer to the plan to execute</param>
        /// <remarks>execute (and equivalents) is the only function in FFTW guaranteed to be thread-safe.</remarks>
        [DllImport(@"x64\libfftw3-3.dll", EntryPoint = "fftw_execute", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void execute(IntPtr plan);

        /// <summary>
        /// Creates a plan for a 1-dimensional complex-to-complex DFT
        /// </summary>
        /// <param name="n">The logical size of the transform</param>
        /// <param name="direction">Specifies the direction of the transform</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll", EntryPoint = "fftw_plan_dft_1d", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_1d(int n, IntPtr input, IntPtr output, InteropFFTWDirection direction, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for a 2-dimensional complex-to-complex DFT
        /// </summary>
        /// <param name="nx">The logical size of the transform along the first dimension</param>
        /// <param name="ny">The logical size of the transform along the second dimension</param>
        /// <param name="direction">Specifies the direction of the transform</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_dft_2d", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_2d(int nx, int ny, IntPtr input, IntPtr output, InteropFFTWDirection direction, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for a 3-dimensional complex-to-complex DFT
        /// </summary>
        /// <param name="nx">The logical size of the transform along the first dimension</param>
        /// <param name="ny">The logical size of the transform along the second dimension</param>
        /// <param name="nz">The logical size of the transform along the third dimension</param>
        /// <param name="direction">Specifies the direction of the transform</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_dft_3d",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_3d(int nx, int ny, int nz, IntPtr input, IntPtr output, InteropFFTWDirection direction, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for an n-dimensional complex-to-complex DFT
        /// </summary>
        /// <param name="rank">Number of dimensions</param>
        /// <param name="n">Array containing the logical size along each dimension</param>
        /// <param name="direction">Specifies the direction of the transform</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_dft", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft(int rank, int[] n, IntPtr input, IntPtr output, InteropFFTWDirection direction, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for a 1-dimensional real-to-complex DFT
        /// </summary>
        /// <param name="n">Number of REAL (input) elements in the transform</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_dft_r2c_1d",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_r2c_1d(int n, IntPtr input, IntPtr output, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for a 2-dimensional real-to-complex DFT
        /// </summary>
        /// <param name="nx">Number of REAL (input) elements in the transform along the first dimension</param>
        /// <param name="ny">Number of REAL (input) elements in the transform along the second dimension</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_dft_r2c_2d",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_r2c_2d(int nx, int ny, IntPtr input, IntPtr output, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for a 3-dimensional real-to-complex DFT
        /// </summary>
        /// <param name="nx">Number of REAL (input) elements in the transform along the first dimension</param>
        /// <param name="ny">Number of REAL (input) elements in the transform along the second dimension</param>
        /// <param name="nz">Number of REAL (input) elements in the transform along the third dimension</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_dft_r2c_3d",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_r2c_3d(int nx, int ny, int nz, IntPtr input, IntPtr output, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for an n-dimensional real-to-complex DFT
        /// </summary>
        /// <param name="rank">Number of dimensions</param>
        /// <param name="n">Array containing the number of REAL (input) elements along each dimension</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_dft_r2c",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_r2c(int rank, int[] n, IntPtr input, IntPtr output, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for a 1-dimensional complex-to-real DFT
        /// </summary>
        /// <param name="n">Number of REAL (output) elements in the transform</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_dft_c2r_1d",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_c2r_1d(int n, IntPtr input, IntPtr output, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for a 2-dimensional complex-to-real DFT
        /// </summary>
        /// <param name="nx">Number of REAL (output) elements in the transform along the first dimension</param>
        /// <param name="ny">Number of REAL (output) elements in the transform along the second dimension</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_dft_c2r_2d",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_c2r_2d(int nx, int ny, IntPtr input, IntPtr output, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for a 3-dimensional complex-to-real DFT
        /// </summary>
        /// <param name="nx">Number of REAL (output) elements in the transform along the first dimension</param>
        /// <param name="ny">Number of REAL (output) elements in the transform along the second dimension</param>
        /// <param name="nz">Number of REAL (output) elements in the transform along the third dimension</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_dft_c2r_3d",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_c2r_3d(int nx, int ny, int nz, IntPtr input, IntPtr output, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for an n-dimensional complex-to-real DFT
        /// </summary>
        /// <param name="rank">Number of dimensions</param>
        /// <param name="n">Array containing the number of REAL (output) elements along each dimension</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_dft_c2r",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_c2r(int rank, int[] n, IntPtr input, IntPtr output, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for a 1-dimensional real-to-real DFT
        /// </summary>
        /// <param name="n">Number of elements in the transform</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="kind">The kind of real-to-real transform to compute</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_r2r_1d",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr r2r_1d(int n, IntPtr input, IntPtr output, InteropFFTWKind kind, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for a 2-dimensional real-to-real DFT
        /// </summary>
        /// <param name="nx">Number of elements in the transform along the first dimension</param>
        /// <param name="ny">Number of elements in the transform along the second dimension</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="kindx">The kind of real-to-real transform to compute along the first dimension</param>
        /// <param name="kindy">The kind of real-to-real transform to compute along the second dimension</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_r2r_2d",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr r2r_2d(int nx, int ny, IntPtr input, IntPtr output, InteropFFTWKind kindx, InteropFFTWKind kindy, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for a 3-dimensional real-to-real DFT
        /// </summary>
        /// <param name="nx">Number of elements in the transform along the first dimension</param>
        /// <param name="ny">Number of elements in the transform along the second dimension</param>
        /// <param name="nz">Number of elements in the transform along the third dimension</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="kindx">The kind of real-to-real transform to compute along the first dimension</param>
        /// <param name="kindy">The kind of real-to-real transform to compute along the second dimension</param>
        /// <param name="kindz">The kind of real-to-real transform to compute along the third dimension</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_r2r_3d",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr r2r_3d(
            int nx, int ny, int nz, IntPtr input, IntPtr output, InteropFFTWKind kindx, InteropFFTWKind kindy, InteropFFTWKind kindz, InteropFFTWFlags flags);

        /// <summary>
        /// Creates a plan for an n-dimensional real-to-real DFT
        /// </summary>
        /// <param name="rank">Number of dimensions</param>
        /// <param name="n">Array containing the number of elements in the transform along each dimension</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="kind">An array containing the kind of real-to-real transform to compute along each dimension</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_plan_r2r",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr r2r(int rank, int[] n, IntPtr input, IntPtr output, InteropFFTWKind[] kind, InteropFFTWFlags flags);

        /// <summary>
        /// Returns (approximately) the number of flops used by a certain plan
        /// </summary>
        /// <param name="plan">The plan to measure</param>
        /// <param name="add">Reference to double to hold number of adds</param>
        /// <param name="mul">Reference to double to hold number of muls</param>
        /// <param name="fma">Reference to double to hold number of fmas (fused multiply-add)</param>
        /// <remarks>Total flops ~= add+mul+2*fma or add+mul+fma if fma is supported</remarks>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_flops",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void flops(IntPtr plan, ref double add, ref double mul, ref double fma);

        /// <summary>
        /// Outputs a "nerd-readable" version of the specified plan to stdout
        /// </summary>
        /// <param name="plan">The plan to output</param>
        [DllImport(@"x64\libfftw3-3.dll",
            EntryPoint = "fftw_print_plan",
            ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void print_plan(IntPtr plan);
    }
}