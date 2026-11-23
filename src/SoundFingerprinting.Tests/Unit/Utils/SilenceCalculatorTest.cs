namespace SoundFingerprinting.Tests.Unit.Utils;

using System.Collections.Generic;
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

    [Test]
    public void ShouldCalculateTheSameSilenceRegardlessOfTheOrderHashesAreEnumeratedIn()
    {
        // non-uniform spacing, as a real stride produces; the silent fingerprints are sequences 3 and 4,
        // so they cover [4.0, 9.5) and the silence is 5.5 s
        var fingerprints = new List<HashedFingerprint>
        {
            Fingerprint(sequenceNumber: 0, startsAt: 0.0f, silent: false),
            Fingerprint(sequenceNumber: 1, startsAt: 1.0f, silent: false),
            Fingerprint(sequenceNumber: 2, startsAt: 2.5f, silent: false),
            Fingerprint(sequenceNumber: 3, startsAt: 4.0f, silent: true),
            Fingerprint(sequenceNumber: 4, startsAt: 7.0f, silent: true),
            Fingerprint(sequenceNumber: 5, startsAt: 9.5f, silent: false),
        };

        var bySequence = SilenceCalculator.Calculate(new Hashes(fingerprints, 10, MediaType.Audio)).TotalSeconds;
        var reversed = SilenceCalculator.Calculate(new Hashes(Enumerable.Reverse(fingerprints).ToList(), 10, MediaType.Audio)).TotalSeconds;

        Assert.Multiple(() =>
        {
            Assert.That(bySequence, Is.EqualTo(5.5).Within(0.01));
            Assert.That(reversed, Is.EqualTo(bySequence).Within(0.01), "the same fingerprints must report the same silence in any enumeration order");
        });
    }

    private static HashedFingerprint Fingerprint(int sequenceNumber, float startsAt, bool silent)
    {
        return new HashedFingerprint(Enumerable.Repeat(silent ? -1 : 1, 25).ToArray(), (uint)sequenceNumber, startsAt, []);
    }
}
