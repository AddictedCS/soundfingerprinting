// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
namespace Soundfingerprinting.Fingerprinting.FFT
{
    // Comments? Questions? Bugs? Tell Ben Houston at ben@exocortex.org
    // Version: May 4, 2002

    /// <summary>
    ///   <p>The direction of the fourier transform.</p>
    /// </summary>
    public enum FourierDirection
    {
        /// <summary>
        ///   Forward direction.  Usually in reference to moving from temporal
        ///   representation to frequency representation
        /// </summary>
        Forward = 1,
        /// <summary>
        ///   Backward direction. Usually in reference to moving from frequency
        ///   representation to temporal representation
        /// </summary>
        Backward = -1,
    }
}