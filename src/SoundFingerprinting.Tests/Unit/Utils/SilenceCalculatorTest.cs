namespace SoundFingerprinting.Tests.Unit.Utils;

using System.Linq;
using NUnit.Framework;
using SoundFingerprinting.Data;
using SoundFingerprinting.Utils;

[TestFixture]
public class SilenceCalculatorTest
{
    [Test]
    public void ShouldCalculateCorrectly()
    {
        var hashedFingerprints = Enumerable.Range(0, 10).Select(index =>
        {
            int[] hashBins = index % 2 == 0 ? Enumerable.Repeat(-1, 25).ToArray() : Enumerable.Repeat(1, 25).ToArray();
            return new HashedFingerprint(hashBins, (uint)index, (float)index / 2, []);
        }).ToArray();

        var hashes = new Hashes(hashedFingerprints, 10, MediaType.Audio);

        var timespan = SilenceCalculator.Calculate(hashes);
        
        Assert.That(timespan.TotalSeconds, Is.EqualTo(2.5).Within(0.1));
    }
}