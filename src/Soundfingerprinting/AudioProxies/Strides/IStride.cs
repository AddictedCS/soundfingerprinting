// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
namespace Soundfingerprinting.AudioProxies.Strides
{
    /// <summary>
    ///   Stride interface
    /// </summary>
    public interface IStride
    {
        /// <summary>
        ///   Get's stride size in terms of number of samples, which need to be skipped
        /// </summary>
        /// <returns>Number samples to skip, between 2 consecutive overlapping fingerprints</returns>
        int GetStride();

        /// <summary>
        ///   Get very first stride
        /// </summary>
        /// <returns>Called at the very beginning just once</returns>
        int GetFirstStride();
    }
}