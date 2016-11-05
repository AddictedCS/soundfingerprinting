namespace SoundFingerprinting.Utils
{
    using System.Collections.Generic;

    using SoundFingerprinting.Strides;

    internal interface ITestRunnerUtils
    {
        List<string> ListFiles(string fromFolder, IEnumerable<string> filters);

        List<int> ParseInts(string cell, char separator);

        IStride ToStride(string stride, string min, string max, int samples);
    }
}