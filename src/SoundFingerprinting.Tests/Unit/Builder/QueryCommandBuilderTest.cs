namespace SoundFingerprinting.Tests.Unit.Builder
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    [TestClass]
    public class QueryCommandBuilderTest : AbstractTest
    {
        private QueryCommandBuilder queryCommandBuilder;

        private Mock<IFingerprintCommandBuilder> fingerprintCommandBuilder;
        private Mock<IQueryFingerprintService> queryFingerprintService;
        private Mock<ISourceFrom> fingerprintingSource;
        private Mock<IWithFingerprintConfiguration> withAlgorithConfiguration;
        private Mock<IFingerprintCommand> fingerprintCommand;
        private Mock<IUsingFingerprintServices> usingFingerprintServices;
        private Mock<IModelService> modelService;
        private Mock<IAudioService> audioService;

        [TestInitialize]
        public void SetUp()
        {
            fingerprintCommandBuilder = new Mock<IFingerprintCommandBuilder>(MockBehavior.Strict);
            fingerprintingSource = new Mock<ISourceFrom>(MockBehavior.Strict);
            withAlgorithConfiguration = new Mock<IWithFingerprintConfiguration>(MockBehavior.Strict);
            fingerprintCommand = new Mock<IFingerprintCommand>(MockBehavior.Strict);
            queryFingerprintService = new Mock<IQueryFingerprintService>(MockBehavior.Strict);
            usingFingerprintServices = new Mock<IUsingFingerprintServices>(MockBehavior.Strict);
            modelService = new Mock<IModelService>(MockBehavior.Strict);
            audioService = new Mock<IAudioService>(MockBehavior.Strict);

            queryCommandBuilder = new QueryCommandBuilder(fingerprintCommandBuilder.Object, queryFingerprintService.Object);
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
            QueryResult dummyResult = new QueryResult(new List<ResultEntry>(), 0);
            List<HashedFingerprint> hashedFingerprints =
                new List<HashedFingerprint>(
                    new[]
                        {
                            new HashedFingerprint(GenericSignature, GenericHashBuckets, 0, 0),
                            new HashedFingerprint(GenericSignature, GenericHashBuckets, 1, 0.928),
                            new HashedFingerprint(GenericSignature, GenericHashBuckets, 2, 0.928 * 2)
                        });

            fingerprintCommandBuilder.Setup(builder => builder.BuildFingerprintCommand()).Returns(fingerprintingSource.Object);
            fingerprintingSource.Setup(source => source.From(PathToFile)).Returns(withAlgorithConfiguration.Object);
            withAlgorithConfiguration.Setup(config => config.WithFingerprintConfig(It.IsAny<EfficientFingerprintConfigurationForQuerying>())).Returns(usingFingerprintServices.Object);
            usingFingerprintServices.Setup(u => u.UsingServices(audioService.Object)).Returns(fingerprintCommand.Object);
            fingerprintCommand.Setup(command => command.Hash()).Returns(Task.Factory.StartNew(() => hashedFingerprints));
            queryFingerprintService.Setup(service => service.Query(modelService.Object, hashedFingerprints, It.IsAny<DefaultQueryConfiguration>())).Returns(dummyResult);

            QueryResult queryResult = queryCommandBuilder.BuildQueryCommand()
                                   .From(PathToFile)
                                   .UsingServices(modelService.Object, audioService.Object)
                                   .Query()
                                   .Result;

            Assert.AreSame(dummyResult, queryResult);
        }

        [TestMethod]
        public void QueryIsBuiltFromFileStartingAtAtSpecificSecondCorrectly()
        {
            const string PathToFile = "path-to-file";
            const int StartAtSecond = 120;
            const int SecondsToQuery = 20;
            QueryResult dummyResult = new QueryResult(new List<ResultEntry>(), 0);
            List<HashedFingerprint> hashDatas =
                new List<HashedFingerprint>(
                    new[]
                        {
                            new HashedFingerprint(GenericSignature, GenericHashBuckets, 0, 0),
                            new HashedFingerprint(GenericSignature, GenericHashBuckets, 1, 0.928),
                            new HashedFingerprint(GenericSignature, GenericHashBuckets, 2, 0.928 * 2)
                        });
            fingerprintCommandBuilder.Setup(builder => builder.BuildFingerprintCommand()).Returns(fingerprintingSource.Object);
            fingerprintingSource.Setup(source => source.From(PathToFile, SecondsToQuery, StartAtSecond)).Returns(withAlgorithConfiguration.Object);
            withAlgorithConfiguration.Setup(config => config.WithFingerprintConfig(It.IsAny<DefaultFingerprintConfiguration>())).Returns(usingFingerprintServices.Object);
            usingFingerprintServices.Setup(u => u.UsingServices(audioService.Object)).Returns(fingerprintCommand.Object);
            fingerprintCommand.Setup(fingerprintingUnit => fingerprintingUnit.Hash()).Returns(Task.Factory.StartNew(() => hashDatas));
            queryFingerprintService.Setup(service => service.Query(modelService.Object, hashDatas, It.IsAny<DefaultQueryConfiguration>())).Returns(dummyResult);

            QueryResult queryResult = queryCommandBuilder.BuildQueryCommand()
                                   .From(PathToFile, SecondsToQuery, StartAtSecond)
                                   .WithConfigs(
                                    config =>
                                       {
                                           config.SpectrogramConfig.LogBase = 64;
                                       },
                                    config =>
                                       {
                                           config.ThresholdVotes = 20;
                                       })
                                   .UsingServices(modelService.Object, audioService.Object)
                                   .Query()
                                   .Result;

            Assert.AreSame(dummyResult, queryResult);
            fingerprintingSource.Verify(source => source.From(PathToFile, SecondsToQuery, StartAtSecond), Times.Once());
        }

        [TestMethod]
        public void QueryCommandIsBuiltWithDefaultConfigsCorrectly()
        {
            var command = queryCommandBuilder.BuildQueryCommand()
                               .From("path-to-file", 10, 0)
                               .UsingServices(modelService.Object, audioService.Object);

            Assert.IsInstanceOfType(command.FingerprintConfiguration, typeof(EfficientFingerprintConfigurationForQuerying));
            Assert.IsInstanceOfType(command.QueryConfiguration, typeof(DefaultQueryConfiguration));
        }

        [TestMethod]
        public void QueryCommandIsBuiltWithCustomConfigsCorrectly()
        {
            var command = queryCommandBuilder.BuildQueryCommand()
                                             .From("path-to-file", 10, 0)
                                             .WithConfigs(
                                                 config =>
                                                     {
                                                         config.SpectrogramConfig.ImageLength = 1024;
                                                     },
                                                 config => 
                                                     {
                                                         config.ThresholdVotes = 256;
                                                     })
                                             .UsingServices(modelService.Object, audioService.Object);

            Assert.AreEqual(1024, command.FingerprintConfiguration.SpectrogramConfig.ImageLength);
            Assert.AreEqual(256, command.QueryConfiguration.ThresholdVotes);
        }
    }
}
