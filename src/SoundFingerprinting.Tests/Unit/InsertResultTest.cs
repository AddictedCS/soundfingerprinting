namespace SoundFingerprinting.Tests.Unit
{
    using System;
    using System.IO;
    using NUnit.Framework;
    using ProtoBuf;

    [TestFixture]
    public class InsertResultTest
    {
        [Test]
        public void UndecidedRoundTripsItsResidentsAndSequence()
        {
            var original = new InsertResult(InsertOutcome.Undecided, undecidedResidentIds: new[] { "a", "b" }, sequence: 42);

            var roundTripped = RoundTrip(original);

            Assert.That(roundTripped.Outcome, Is.EqualTo(InsertOutcome.Undecided));
            Assert.That(roundTripped.UndecidedResidentIds, Is.EqualTo(new[] { "a", "b" }));
            Assert.That(roundTripped.Sequence, Is.EqualTo(42));
            Assert.That(roundTripped.Undecided, Is.True);
            Assert.That(roundTripped.Persisted, Is.False);
            Assert.That(roundTripped.DuplicatePrevented, Is.False);
        }

        [Test]
        public void InsertedWithoutTheNewMembersReadsNulls()
        {
            var roundTripped = RoundTrip(new InsertResult(InsertOutcome.Inserted));

            Assert.That(roundTripped.Outcome, Is.EqualTo(InsertOutcome.Inserted));
            Assert.That(roundTripped.UndecidedResidentIds, Is.Null);
            Assert.That(roundTripped.Sequence, Is.Null);
            Assert.That(roundTripped.Persisted, Is.True);
        }

        [Test]
        public void InsertedCarriesAnInformationalSequence()
        {
            var roundTripped = RoundTrip(new InsertResult(InsertOutcome.Inserted, sequence: 7));

            Assert.That(roundTripped.Sequence, Is.EqualTo(7));
            Assert.That(roundTripped.Persisted, Is.True);
        }

        [Test]
        public void UndecidedRequiresResidentIdsAndASequence()
        {
            Assert.Throws<ArgumentException>(() => new InsertResult(InsertOutcome.Undecided, sequence: 1));
            Assert.Throws<ArgumentException>(() => new InsertResult(InsertOutcome.Undecided, undecidedResidentIds: Array.Empty<string>(), sequence: 1));
            Assert.Throws<ArgumentException>(() => new InsertResult(InsertOutcome.Undecided, undecidedResidentIds: new[] { "a" }));
        }

        [Test]
        public void DuplicateSuppressedRequiresTheDuplicateId()
        {
            Assert.Throws<ArgumentException>(() => new InsertResult(InsertOutcome.DuplicateSuppressed));
            Assert.That(new InsertResult(InsertOutcome.DuplicateSuppressed, "owner").DuplicatePrevented, Is.True);
        }

        private static InsertResult RoundTrip(InsertResult original)
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, original);
            stream.Position = 0;
            return Serializer.Deserialize<InsertResult>(stream);
        }
    }
}
