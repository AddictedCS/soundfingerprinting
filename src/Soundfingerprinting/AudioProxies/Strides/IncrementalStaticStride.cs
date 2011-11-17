// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
namespace Soundfingerprinting.AudioProxies.Strides
{
    /// <summary>
    ///   Incremental stride
    /// </summary>
    public class IncrementalStaticStride : IStride
    {
        /// <summary>
        ///   Increment by parameter (usually negative)
        /// </summary>
        private readonly int _incrementBy;

        private int _firstStride;

        /// <summary>
        ///   Incremental stride constructor
        /// </summary>
        /// <param name = "incrementBy">Increment by parameter in audio samples</param>
        /// <param name = "samplesInFingerprint">Number of samples in one fingerprint [normally 8192]</param>
        public IncrementalStaticStride(int incrementBy, int samplesInFingerprint)
        {
            _incrementBy = -samplesInFingerprint + incrementBy; /*Negative stride will guarantee that the signal is incremented by the parameter specified*/
            _firstStride = 0;
        }

        #region IStride Members

        /// <summary>
        ///   Gets stride size
        /// </summary>
        /// <returns>Negative stride</returns>
        public int GetStride()
        {
            return _incrementBy;
        }

        public int GetFirstStride()
        {
            return _firstStride;
        }

        #endregion

        public void SetFirstStride(int firstStride)
        {
            _firstStride = firstStride;
        }
    }
}