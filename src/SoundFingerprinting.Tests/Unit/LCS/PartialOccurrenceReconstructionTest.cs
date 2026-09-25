namespace SoundFingerprinting.Tests.Unit.LCS;

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Configuration;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;
using SoundFingerprinting.LCS;
using SoundFingerprinting.Query;

[TestFixture]
public class PartialOccurrenceReconstructionTest
{
    [TestCase(false, 0)]
    [TestCase(false, 100)]
    [TestCase(true, 0)]
    [TestCase(true, 100)]
    public void ShouldRecoverAnEarlierSuffixBesideALaterPrefix(bool reverseAxes, int queryShift)
    {
        var pairs = Enumerable.Range(0, 4).Select(i => (Q: i + 10, T: i))
            .Concat(Enumerable.Range(0, 6).Select(i => (Q: i, T: i + 10)))
            .Select(p => reverseAxes ? (Q: p.T, T: p.Q) : p)
            .Select(p => new MatchedWith((uint)(p.Q + queryShift), p.Q + queryShift, (uint)p.T, p.T, 1))
            .ToArray();

        var paths = new QueryPathReconstructionStrategy().GetBestPaths(pairs, 5).Select(p => p.ToArray()).ToArray();

        Assert.That(paths.Select(p => p.Length), Is.EqualTo(new[] { 6, 4 }));
        Assert.That(paths.SelectMany(p => p), Is.EquivalentTo(pairs));
        foreach (var path in paths)
        {
            Assert.That(path.Select(p => p.TrackMatchAt - p.QueryMatchAt).Distinct().Count(), Is.EqualTo(1));
        }
    }

    [TestCase(false, 0, 0)]
    [TestCase(false, 100, 0)]
    [TestCase(false, 0, 100)]
    [TestCase(true, 0, 0)]
    [TestCase(true, 100, 0)]
    [TestCase(true, 0, 100)]
    public void ShouldPreservePartialAndFullOccurrences(bool reverseAxes, int queryShift, int trackShift)
    {
        var pairs = Enumerable.Range(0, 3).Select(i => (Q: i, T: i))
            .Concat(Enumerable.Range(0, 6).Select(i => (Q: i, T: i + 4)))
            .Select(p => reverseAxes ? (Q: p.T, T: p.Q) : p)
            .Select(p => new MatchedWith((uint)(p.Q + queryShift), p.Q + queryShift, (uint)(p.T + trackShift), p.T + trackShift, 1))
            .ToArray();

        foreach (var input in new[] { pairs, pairs.Reverse().ToArray(), pairs.OrderBy(p => p.QuerySequenceNumber).ToArray() })
        {
            var paths = new QueryPathReconstructionStrategy().GetBestPaths(input, 5).Select(p => p.ToArray()).ToArray();

            Assert.That(paths.Select(p => p.Length), Is.EqualTo(new[] { 6, 3 }));
            Assert.That(paths.SelectMany(p => p), Is.EquivalentTo(pairs));
            foreach (var path in paths)
            {
                Assert.That(path.Select(p => p.TrackMatchAt - p.QueryMatchAt).Distinct().Count(), Is.EqualTo(1));
                Assert.That(path.Select(p => p.QuerySequenceNumber).Distinct().Count(), Is.EqualTo(path.Length));
                Assert.That(path.Select(p => p.TrackSequenceNumber).Distinct().Count(), Is.EqualTo(path.Length));
            }
        }
    }

    [TestCase(false, 0)]
    [TestCase(false, 4)]
    [TestCase(true, 0)]
    [TestCase(true, 4)]
    public void ShouldStartANewPathAfterDistantCrossMatches(bool reverseAxes, int creativeOffset)
    {
        var occurrence = Enumerable.Range(0, 10).Select(i => (Q: i + creativeOffset, T: i + 30));
        var pairs = new[] { (Q: 5, T: 0), (Q: 6, T: 1), (Q: 7, T: 2), (Q: 3, T: 7), (Q: 7, T: 19), (Q: 8, T: 20), (Q: 3, T: 43) }
            .Concat(occurrence)
            .Select(p => reverseAxes ? (Q: p.T, T: p.Q) : p)
            .Select(p => new MatchedWith((uint)p.Q, p.Q, (uint)p.T, p.T, 1));

        var path = new QueryPathReconstructionStrategy().GetBestPaths(pairs, 5).First().ToArray();

        Assert.That(path, Has.Length.EqualTo(10));
        Assert.That(path.Select(p => (p.QueryMatchAt, p.TrackMatchAt)),
            Is.EqualTo(occurrence.Select(p => reverseAxes ? ((float)p.T, (float)p.Q) : ((float)p.Q, (float)p.T))));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ShouldRecoverTheCutOffAiringAmongUnrelatedMatches(bool reverseAxes)
    {
        const float stride = 0.068f;
        var early = Enumerable.Range(0, 118).Select(i => (Q: i, T: i)).ToArray();
        var later = Enumerable.Range(0, 441).Select(i => (Q: i, T: i + 118)).ToArray();
        var random = new Random(4);
        var noise = new System.Collections.Generic.HashSet<(int Q, int T)>();
        while (noise.Count < 300)
        {
            noise.Add((random.Next(441), random.Next(1764)));
        }

        MatchedWith Match((int Q, int T) p) => reverseAxes
            ? new MatchedWith((uint)p.T, p.T * stride, (uint)p.Q, p.Q * stride, 1)
            : new MatchedWith((uint)p.Q, p.Q * stride, (uint)p.T, p.T * stride, 1);
        var matches = early.Concat(later).Concat(noise).Distinct().Select(Match);

        var paths = new QueryPathReconstructionStrategy().GetBestPaths(matches, 5).Select(p => p.ToArray()).ToArray();

        Assert.That(paths.Any(p => early.Select(Match).All(p.Contains)), Is.True, "the early occurrence must survive competitor exclusion");
        Assert.That(paths.Any(p => later.Select(Match).All(p.Contains)), Is.True, "the later occurrence must remain intact");
    }

    [Test]
    public void ShouldRetainHeadCompetitorExclusionForIncompatibleHits()
    {
        var matches = Enumerable.Range(0, 2000).Select(i => new MatchedWith((uint)i, i * 0.068f, (uint)(2000 - i), (2000 - i) * 0.068f, 1));

        var paths = new QueryPathReconstructionStrategy().GetBestPaths(matches, 5).ToArray();

        Assert.That(paths, Has.Length.EqualTo(1));
        Assert.That(paths[0].Count(), Is.EqualTo(1));
    }

    [Test]
    public void ShouldKeepCompetitorSuppressionWhenRecoveringCrossedPrefixes()
    {
        const int groups = 1000;
        var matches = Enumerable.Range(0, groups).SelectMany(i => Enumerable.Range(0, 2)
            .Select(j => new MatchedWith((uint)(2 * (groups - i) + j), (2 * (groups - i) + j) * 0.068f,
                (uint)(2 * i + j), (2 * i + j) * 0.068f, 1)));

        var paths = new QueryPathReconstructionStrategy().GetBestPaths(matches, 5).ToArray();

        // each axis can recover one branch; the other 998 remain competitors rather than separate results.
        Assert.That(paths, Has.Length.EqualTo(2));
        Assert.That(paths.Select(p => p.Count()), Is.EqualTo(new[] { 2, 2 }));
    }

    [TestCase(100, 100, 1)]
    [TestCase(147, 441, 4)]
    [TestCase(441, 1764, 5)]
    public void ShouldKeepDenseAmbiguityAsCompetingPaths(int queryCount, int trackCount, int expectedPaths)
    {
        var matches = Enumerable.Range(0, queryCount).SelectMany(q => Enumerable.Range(0, trackCount)
            .Select(t => new MatchedWith((uint)q, q * 0.068f, (uint)t, t * 0.068f, 1)));

        var paths = new QueryPathReconstructionStrategy().GetBestPaths(matches, 5).Select(p => p.ToArray()).ToArray();

        Assert.That(paths, Has.Length.EqualTo(expectedPaths));
        Assert.That(paths[0], Has.Length.EqualTo(queryCount));
    }

    [TestCase(30, 30, false)]
    [TestCase(30, 30, true)]
    [TestCase(8, 8, false)]
    [TestCase(8, 8, true)]
    [TestCase(8, 30, false)]
    [TestCase(30, 8, false)]
    [TestCase(8, 30, true)]
    [TestCase(30, 8, true)]
    public async Task ShouldRecoverCutOffAiringBesideAFullAiring(int firstLength, int secondLength, bool queryBroadcast)
    {
        const int sampleRate = 5512;
        var random = new Random(17);
        float[] samples = Enumerable.Range(0, 30 * sampleRate).Select(_ => (float)random.NextDouble() * 2 - 1).ToArray();
        var creative = new AudioSamples(samples, string.Empty, sampleRate);
        var broadcast = new AudioSamples(samples.Take(firstLength * sampleRate).Concat(samples.Take(secondLength * sampleRate)).ToArray(), string.Empty, sampleRate);
        var creativeHashes = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand().From(creative).Hash();
        var broadcastHashes = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand().From(broadcast).Hash();
        var model = new InMemoryModelService();
        model.Insert(new TrackInfo("stored", string.Empty, string.Empty), queryBroadcast ? creativeHashes : broadcastHashes);
        var query = (queryBroadcast ? broadcastHashes : creativeHashes).Audio!;

        var results = QueryFingerprintService.Instance.Query(query, new DefaultQueryConfiguration { ThresholdVotes = 4, PermittedGap = 5 }, model)
            .ResultEntries.Select(r => r.Coverage).OrderBy(c => queryBroadcast ? c.QueryMatchStartsAt : c.TrackMatchStartsAt).ToArray();

        Assert.That(results, Has.Length.EqualTo(2));
        Assert.That(queryBroadcast ? results[0].QueryMatchStartsAt : results[0].TrackMatchStartsAt, Is.EqualTo(0).Within(0.2));
        Assert.That(queryBroadcast ? results[1].QueryMatchStartsAt : results[1].TrackMatchStartsAt, Is.EqualTo(firstLength).Within(0.2));
        Assert.That(queryBroadcast ? results[0].TrackCoverageWithPermittedGapsLength : results[0].QueryCoverageWithPermittedGapsLength,
            Is.InRange(firstLength - 1d, firstLength + 0.2));
        Assert.That(queryBroadcast ? results[1].TrackCoverageWithPermittedGapsLength : results[1].QueryCoverageWithPermittedGapsLength,
            Is.InRange(secondLength - 1d, secondLength + 0.2));
    }
}
