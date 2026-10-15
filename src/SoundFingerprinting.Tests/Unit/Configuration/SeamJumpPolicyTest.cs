namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System;
    using NUnit.Framework;
    using SoundFingerprinting.Configuration;

    [TestFixture]
    public class SeamJumpPolicyTest
    {
        [TestCase(-1)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void ShouldRejectACeilingThatIsNotAFiniteNonNegativeNumber(double ceiling)
        {
            Assert.That(() => SeamJumpPolicy.NewOccurrenceAbove(ceiling), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ShouldDefaultToStitchAndMapTheSkipFlagOntoThePolicy()
        {
            var config = new DefaultRealtimeQueryConfiguration(_ => { }, _ => { }, (_, _) => { }, () => { });
            Assert.That(config.SeamJumpPolicy, Is.SameAs(SeamJumpPolicy.Stitch));
#pragma warning disable CS0618
            Assert.That(config.AutomaticSkipDetection, Is.False);

            config.AutomaticSkipDetection = true;
            Assert.That(config.SeamJumpPolicy, Is.SameAs(SeamJumpPolicy.SourceSkip));
            Assert.That(config.AutomaticSkipDetection, Is.True);

            config.AutomaticSkipDetection = false;
            Assert.That(config.SeamJumpPolicy, Is.SameAs(SeamJumpPolicy.Stitch));

            // the flag must not erase a ceiling it did not set
            config.SeamJumpPolicy = SeamJumpPolicy.NewOccurrenceAbove(120);
            config.AutomaticSkipDetection = false;
            Assert.That(config.SeamJumpPolicy.Kind, Is.EqualTo(SeamJumpKind.NewOccurrence));
            Assert.That(config.SeamJumpPolicy.CeilingSeconds, Is.EqualTo(120));
            Assert.That(config.AutomaticSkipDetection, Is.False);
#pragma warning restore CS0618
        }
    }
}
