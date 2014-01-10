namespace SoundFingerprinting.Tests.Unit.Fingerprinting
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Query.Configuration;

    [TestClass]
    public class FingerprintQueryBuilderTest : AbstractTest
    {
        private FingerprintQueryBuilder fingerprintQueryBuilder;

        private Mock<IFingerprintCommandBuilder> fingerprintCommandBuilder;
        private Mock<IQueryFingerprintService> queryFingerprintService;

        private Mock<ISourceFrom> fingerprintingSource;
        private Mock<IWithFingerprintConfiguration> withAlgorithConfiguration;

        private Mock<IFingerprintCommand> fingerprintCommand;

        [TestInitialize]
        public void SetUp()
        {
            fingerprintCommandBuilder = new Mock<IFingerprintCommandBuilder>(MockBehavior.Strict);
            fingerprintingSource = new Mock<ISourceFrom>(MockBehavior.Strict);
            withAlgorithConfiguration = new Mock<IWithFingerprintConfiguration>(MockBehavior.Strict);
            fingerprintCommand = new Mock<IFingerprintCommand>(MockBehavior.Strict);
            queryFingerprintService = new Mock<IQueryFingerprintService>(MockBehavior.Strict);
            
            fingerprintQueryBuilder = new FingerprintQueryBuilder(fingerprintCommandBuilder.Object, queryFingerprintService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            fingerprintCommandBuilder.VerifyAll();
            fingerprintingSource.VerifyAll();
            withAlgorithConfiguration.VerifyAll();
            fingerprintCommand.VerifyAll();
            queryFingerprintService.VerifyAll();
        }

        [TestMethod]
        public void QueryIsBuiltFromFileCorrectly()
        {
            const string PathToFile = "path-to-file";
            QueryResult dummyResult = new QueryResult { IsSuccessful = true, BestMatch = It.IsAny<TrackData>() };
            List<HashData> hashDatas = new List<HashData>(new[] { new HashData(GenericSignature, GenericHashBuckets), new HashData(GenericSignature, GenericHashBuckets), new HashData(GenericSignature, GenericHashBuckets) });
            fingerprintCommandBuilder.Setup(builder => builder.BuildFingerprintCommand()).Returns(fingerprintingSource.Object);
            fingerprintingSource.Setup(source => source.From(PathToFile)).Returns(withAlgorithConfiguration.Object);
            withAlgorithConfiguration.Setup(config => config.WithDefaultAlgorithmConfiguration()).Returns(fingerprintCommand.Object);
            fingerprintCommand.Setup(fingerprintingUnit => fingerprintingUnit.Hash()).Returns(Task.Factory.StartNew(() => hashDatas));
            queryFingerprintService.Setup(service => service.Query(hashDatas, It.IsAny<DefaultQueryConfiguration>())).Returns(dummyResult);

            QueryResult queryResult = fingerprintQueryBuilder.BuildQuery()
                                   .From(PathToFile)
                                   .WithDefaultConfigurations()
                                   .Query()
                                   .Result;

            Assert.AreEqual(dummyResult, queryResult);
        }

        [TestMethod]
        public void QueryIsBuiltFromFileStartingAtAtSpecificSecondCorrectly()
        {
            const string PathToFile = "path-to-file";
            const int StartAtSecond = 120;
            const int SecondsToQuery = 20;
            QueryResult dummyResult = new QueryResult { IsSuccessful = true, BestMatch = It.IsAny<TrackData>() };
            List<HashData> hashDatas = new List<HashData>(new[] { new HashData(GenericSignature, GenericHashBuckets), new HashData(GenericSignature, GenericHashBuckets), new HashData(GenericSignature, GenericHashBuckets) });
            fingerprintCommandBuilder.Setup(builder => builder.BuildFingerprintCommand()).Returns(fingerprintingSource.Object);
            fingerprintingSource.Setup(source => source.From(PathToFile, SecondsToQuery, StartAtSecond)).Returns(withAlgorithConfiguration.Object);
            withAlgorithConfiguration.Setup(config => config.WithDefaultAlgorithmConfiguration()).Returns(fingerprintCommand.Object);
            fingerprintCommand.Setup(fingerprintingUnit => fingerprintingUnit.Hash()).Returns(Task.Factory.StartNew(() => hashDatas));
            queryFingerprintService.Setup(service => service.Query(hashDatas, It.IsAny<DefaultQueryConfiguration>())).Returns(dummyResult);

            QueryResult queryResult = fingerprintQueryBuilder.BuildQuery()
                                   .From(PathToFile, SecondsToQuery, StartAtSecond)
                                   .WithDefaultConfigurations()
                                   .Query()
                                   .Result;

            Assert.AreEqual(dummyResult, queryResult);
            fingerprintingSource.Verify(source => source.From(PathToFile, SecondsToQuery, StartAtSecond), Times.Once());
        }
    }
}
