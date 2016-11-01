namespace SoundFingerprinting.Tests.Unit.Query
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Query;

    [TestClass]
    public class QueryMathTest
    {
        private readonly QueryMath queryMath = new QueryMath();

        [TestMethod]
        public void ShouldAdjustQueryResultSnippetLengthCorrectly()
        {
            var fc = new DefaultFingerprintConfiguration();

            double snippetLength = queryMath.AdjustSnippetLengthToConfigsUsedDuringFingerprinting(8.142235, fc);

            Assert.AreEqual(10d, snippetLength, 0.0001);
        }
    }
}
