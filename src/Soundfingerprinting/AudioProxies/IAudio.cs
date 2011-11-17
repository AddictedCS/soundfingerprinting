// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;

namespace Soundfingerprinting.AudioProxies
{
    /// <summary>
    ///   Digital signal processing proxy
    /// </summary>
    public interface IAudio : IDisposable
    {
        /// <summary>
        ///   Read from file at a specific frequency rate
        /// </summary>
        /// <param name = "filename">Filename to read from</param>
        /// <param name = "samplerate">Sample rate</param>
        /// <param name = "milliseconds">Milliseconds to read</param>
        /// <param name = "startmilliseconds">Start at a specific millisecond range</param>
        /// <returns>Array with data</returns>
        float[] ReadMonoFromFile(string filename, int samplerate, int milliseconds, int startmilliseconds);
    }
}