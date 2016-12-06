namespace SoundFingerprinting.Utils
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using SoundFingerprinting.Strides;

    internal class TestRunnerUtils : ITestRunnerUtils
    {
        public List<string> ListFiles(string fromFolder, IEnumerable<string> filters)
        {
            return
                filters.SelectMany(filter => Directory.GetFiles(fromFolder, filter, SearchOption.AllDirectories))
                      .ToList();
        }

        public List<int> ParseInts(string cell, char separator)
        {
            return cell.Split(separator).Select(int.Parse).ToList();
        }

        public IStride ToStride(string stride, string min, string max)
        {
            return StrideUtils.ToStride(stride, int.Parse(min), int.Parse(max));
        }
    }
}
