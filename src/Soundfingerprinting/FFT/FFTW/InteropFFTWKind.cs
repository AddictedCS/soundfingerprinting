namespace Soundfingerprinting.FFT.FFTW
{
    /// <summary>
    /// Kinds of real-to-real transforms
    /// </summary>
    internal enum InteropFFTWKind : uint
    {
// ReSharper disable InconsistentNaming
        R2HC = 0,
        HC2R = 1,
        DHT = 2,
        REDFT00 = 3,
        REDFT01 = 4,
        REDFT10 = 5,
        REDFT11 = 6,
        RODFT00 = 7,
        RODFT01 = 8,
        RODFT10 = 9,
        RODFT11 = 10
// ReSharper restore InconsistentNaming
    }
}