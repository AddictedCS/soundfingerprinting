namespace SoundFingerprinting.Tests.Unit.Fingerprinting
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Query;
    using SoundFingerprinting.Query.Configuration;

    [TestClass]
    public class FingerprintQueryBuilderTest : AbstractTest
    {
        private FingerprintQueryBuilder fingerprintQueryBuilder;

        private Mock<IFingerprintUnitBuilder> fingerprintUnitBuilder;
        private Mock<IQueryFingerprintService> queryFingerprintService;

        private Mock<ISourceFrom> fingerprintingSource;
        private Mock<IWithAlgorithmConfiguration> withAlgorithConfiguration;

        private Mock<IAudioFingerprintingUnit> audioFingerprintingUnit;
        private Mock<IFingerprinter> fingerprinter;

        [TestInitialize]
        public void SetUp()
        {
            fingerprintUnitBuilder = new Mock<IFingerprintUnitBuilder>(MockBehavior.Strict);
            fingerprintingSource = new Mock<ISourceFrom>(MockBehavior.Strict);
            withAlgorithConfiguration = new Mock<IWithAlgorithmConfiguration>(MockBehavior.Strict);
            audioFingerprintingUnit = new Mock<IAudioFingerprintingUnit>(MockBehavior.Strict);
            fingerprinter = new Mock<IFingerprinter>(MockBehavior.Strict);
            queryFingerprintService = new Mock<IQueryFingerprintService>(MockBehavior.Strict);
            
            fingerprintQueryBuilder = new FingerprintQueryBuilder(fingerprintUnitBuilder.Object, queryFingerprintService.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            fingerprintUnitBuilder.VerifyAll();
            fingerprintingSource.VerifyAll();
            withAlgorithConfiguration.VerifyAll();
            audioFingerprintingUnit.VerifyAll();
            fingerprinter.VerifyAll();
            queryFingerprintService.VerifyAll();
        }

        [TestMethod]
        public void QueryIsBuiltFromFileCorrectly()
        {
            const string PathToFile = "path-to-file";
            QueryResult dummyResult = new QueryResult { IsSuccessful = true, Results = It.IsAny<List<ResultData>>() };
            List<bool[]> rawFingerprints = new List<bool[]>(new[] { GenericFingerprint, GenericFingerprint, GenericFingerprint });
            fingerprintUnitBuilder.Setup(builder => builder.BuildAudioFingerprintingUnit()).Returns(fingerprintingSource.Object);
            fingerprintingSource.Setup(source => source.From(PathToFile)).Returns(withAlgorithConfiguration.Object);
            withAlgorithConfiguration.Setup(config => config.WithDefaultAlgorithmConfiguration()).Returns(audioFingerprintingUnit.Object);
            audioFingerprintingUnit.Setup(fingerprintingUnit => fingerprintingUnit.FingerprintIt()).Returns(fingerprinter.Object);
            fingerprinter.Setup(finger => finger.AsIs()).Returns(Task<List<bool[]>>.Factory.StartNew(() => rawFingerprints));
            queryFingerprintService.Setup(service => service.Query(rawFingerprints, It.IsAny<DefaultQueryConfiguration>())).Returns(dummyResult);  

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
            QueryResult dummyResult = new QueryResult { IsSuccessful = true, Results = It.IsAny<List<ResultData>>() };
            List<bool[]> rawFingerprints = new List<bool[]>(new[] { GenericFingerprint, GenericFingerprint, GenericFingerprint });
            fingerprintUnitBuilder.Setup(builder => builder.BuildAudioFingerprintingUnit()).Returns(fingerprintingSource.Object);
            fingerprintingSource.Setup(source => source.From(PathToFile, SecondsToQuery, StartAtSecond)).Returns(withAlgorithConfiguration.Object);
            withAlgorithConfiguration.Setup(config => config.WithDefaultAlgorithmConfiguration()).Returns(audioFingerprintingUnit.Object);
            audioFingerprintingUnit.Setup(fingerprintingUnit => fingerprintingUnit.FingerprintIt()).Returns(fingerprinter.Object);
            fingerprinter.Setup(finger => finger.AsIs()).Returns(Task<List<bool[]>>.Factory.StartNew(() => rawFingerprints));
            queryFingerprintService.Setup(service => service.Query(rawFingerprints, It.IsAny<DefaultQueryConfiguration>())).Returns(dummyResult);

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
