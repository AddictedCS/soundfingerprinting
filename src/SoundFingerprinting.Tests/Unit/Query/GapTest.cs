using NUnit.Framework;
using SoundFingerprinting.Query;
using System;

namespace SoundFingerprinting.Tests.Unit.Query;

using System.Linq;
using System.Threading.Tasks;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;

[TestFixture]
public class GapTest
{
    [Test]
    public void ConstructorThrowsOnInvalidArgs()
    {
        Assert.Throws<ArgumentException>(() => new Gap(4, 3, false));
    }

    [Test]
    public void LengthMustBePositive()
    {
        Assert.AreEqual(42, new Gap(8, 50, false).LengthInSeconds);
    }

    [Test]
    public async Task ShouldIdentifyGaps()
    {
        int seconds = 60 * 5, offsetSeconds = 30, differenceLength = 10, sampleRate = 5512, secondGapOffset = 150, thirdGapOffset = 290;
        var original = TestUtilities.GenerateRandomAudioSamples(seconds * sampleRate, sampleRate: sampleRate);
        var buffer = new float[original.Samples.Length];
        Array.Copy(original.Samples, buffer, original.Samples.Length);
        for (int i = 0; i < differenceLength * sampleRate; ++i)
        {
            // introduce a difference in the middle of the buffer
            // starting with offsetSeconds, of length differenceLength
            buffer[i + offsetSeconds * sampleRate] = Random.Shared.NextSingle();
            buffer[i + secondGapOffset * sampleRate] = Random.Shared.NextSingle();
            buffer[i + thirdGapOffset * sampleRate] = Random.Shared.NextSingle();
        }

        var modelService = new InMemoryModelService();
        var avHashes = await FingerprintCommandBuilder.Instance
            .BuildFingerprintCommand()
            .From(original)
            .UsingServices(new SoundFingerprintingAudioService())
            .Hash();
            
        // insert the original track
        var track = new TrackInfo("id", "title", "artist");
        modelService.Insert(track, avHashes);

        var (audioResult, _) = await QueryCommandBuilder
            .Instance
            .BuildQueryCommand()
            .From(new AudioSamples(buffer, string.Empty, sampleRate))
            .UsingServices(modelService, new SoundFingerprintingAudioService())
            .Query();
            
        Assert.That(audioResult, Is.Not.Null);
        Assert.That(audioResult.ContainsMatches, Is.True);

        var match = audioResult.BestMatch;
        Assert.That(match, Is.Not.Null);
            
        Assert.That(match.Coverage.QueryGaps, Is.Not.Empty);
        Assert.That(match.Coverage.TrackGaps, Is.Not.Empty);
        Assert.That(match.Coverage.QueryGaps.Count(), Is.EqualTo(3));
        Assert.That(match.Coverage.TrackGaps.Count(), Is.EqualTo(3));
        
        var queryGaps = match.Coverage.QueryGaps.ToList();
        var trackGaps = match.Coverage.TrackGaps.ToList();
            
        AssertGap(queryGaps[0], differenceLength, trackGaps[0], offsetSeconds);
        AssertGap(queryGaps[1], differenceLength, trackGaps[1], secondGapOffset);
        AssertGap(queryGaps[2], differenceLength, trackGaps[2], thirdGapOffset);
    }

    private static void AssertGap(Gap queryGap, int gapLength, Gap trackGap, int gapStartsAt)
    {
        Assert.That(queryGap.LengthInSeconds, Is.EqualTo(gapLength).Within(1d));
        Assert.That(trackGap.LengthInSeconds, Is.EqualTo(gapLength).Within(1d));
        Assert.That(queryGap.Start, Is.EqualTo(gapStartsAt).Within(1d));
        Assert.That(trackGap.Start, Is.EqualTo(gapStartsAt).Within(1d));
    }
}