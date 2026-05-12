namespace SoundFingerprinting.Tests.Unit.Configuration;

using NUnit.Framework;
using SoundFingerprinting.Configuration;
using SoundFingerprinting.LCS;
using SoundFingerprinting.SFM;

[TestFixture]
public class QueryConfigurationCascadeTest
{
    [Test]
    public void SettingNonNoBridgingStrategyShouldFlipComputeSpectralProfileOn()
    {
        var config = new DefaultQueryConfiguration();
        Assume.That(config.FingerprintConfiguration.ComputeSpectralProfile, Is.False);

        config.SfmMatchStrategy = BroadbandNoiseBridgingStrategy.Default;

        Assert.That(config.FingerprintConfiguration.ComputeSpectralProfile, Is.True);
    }

    [Test]
    public void SettingBackToNoBridgingShouldFlipComputeSpectralProfileOff()
    {
        var config = new DefaultQueryConfiguration
        {
            SfmMatchStrategy = SilentRegionBridgingStrategy.Default,
        };
        Assume.That(config.FingerprintConfiguration.ComputeSpectralProfile, Is.True);

        config.SfmMatchStrategy = NoBridgingStrategy.Default;

        Assert.That(config.FingerprintConfiguration.ComputeSpectralProfile, Is.False);
    }

    [Test]
    public void DefaultStrategyShouldBeNoBridging()
    {
        var config = new DefaultQueryConfiguration();

        Assert.That(config.SfmMatchStrategy, Is.InstanceOf<NoBridgingStrategy>());
    }
}
