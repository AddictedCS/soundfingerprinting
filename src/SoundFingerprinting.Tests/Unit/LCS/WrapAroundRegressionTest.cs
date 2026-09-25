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
using SoundFingerprinting.Query;

[TestFixture]
public class WrapAroundRegressionTest
{
    [Test]
    public async Task ShouldRecoverBothHalvesWhenQueryWrapsAroundStoredCreative()
    {
        const int sampleRate = 5512;
        var samples = new[] { 11, 22, 33, 44 }.SelectMany(seed =>
        {
            var random = new Random(seed);
            return Enumerable.Range(0, 5 * sampleRate).Select(_ => (float)(random.NextDouble() * 2 - 1));
        }).ToArray();
        var rotated = samples.Skip(10 * sampleRate).Concat(samples.Take(10 * sampleRate)).ToArray();
        var creative = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
            .From(new AudioSamples(samples, string.Empty, sampleRate)).Hash();
        var broadcast = await FingerprintCommandBuilder.Instance.BuildFingerprintCommand()
            .From(new AudioSamples(rotated, string.Empty, sampleRate)).Hash();
        var model = new InMemoryModelService();
        model.Insert(new TrackInfo("stored", string.Empty, string.Empty), creative);
        var query = broadcast.Audio!;
        var config = new DefaultQueryConfiguration
        {
            ThresholdVotes = 3,
            TruePositivesFilter = new TruePositivesFilter(null, 0.25, null)
        };

        var results = QueryFingerprintService.Instance.Query(query, config, model)
            .ResultEntries.OrderBy(r => r.QueryMatchStartsAt).ToArray();

        Assert.That(results, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(results[0].QueryMatchStartsAt, Is.EqualTo(0).Within(0.2));
            Assert.That(results[0].TrackMatchStartsAt, Is.EqualTo(10).Within(0.2));
            Assert.That(results[1].QueryMatchStartsAt, Is.EqualTo(10).Within(0.2));
            Assert.That(results[1].TrackMatchStartsAt, Is.EqualTo(0).Within(0.2));
            Assert.That(results.All(r => r.Coverage.QueryDiscreteCoverageLength > 9), Is.True);
        });
    }
}
