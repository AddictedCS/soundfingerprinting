// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
namespace Soundfingerprinting.SoundTools
{
    /// <summary>
    ///   Possible hash algorithms
    /// </summary>
    public enum HashAlgorithm
    {
        /// <summary>
        ///   Locality Sensitive Hashing + Min Hash
        /// </summary>
        LSH = 0,

        /// <summary>
        ///   Neural Hasher
        /// </summary>
        NeuralHasher = 1,

        /// <summary>
        ///   No Hash
        /// </summary>
        None = 2
    }
}