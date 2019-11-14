namespace SoundFingerprinting.Tests.Unit.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Query;

    [TestFixture]
    public class QueryCommandBuilderTest : AbstractTest
    {
        private QueryCommandBuilder queryCommandBuilder;

        private Mock<IFingerprintCommandBuilder> fingerprintCommandBuilder;
        private Mock<IQueryFingerprintService> queryFingerprintService;
        private Mock<ISourceFrom> fingerprintingSource;
        private Mock<IWithFingerprintConfiguration> withAlgorithmConfiguration;
        private Mock<IFingerprintCommand> fingerprintCommand;
        private Mock<IUsingFingerprintServices> usingFingerprintServices;
        private Mock<IModelService> modelService;
        private Mock<IAudioService> audioService;

        [SetUp]
        public void SetUp()
        {
            fingerprintCommandBuilder = new Mock<IFingerprintCommandBuilder>(MockBehavior.Strict);
            fingerprintingSource = new Mock<ISourceFrom>(MockBehavior.Strict);
            withAlgorithmConfiguration = new Mock<IWithFingerprintConfiguration>(MockBehavior.Strict);
            fingerprintCommand = new Mock<IFingerprintCommand>(MockBehavior.Strict);
            queryFingerprintService = new Mock<IQueryFingerprintService>(MockBehavior.Strict);
            usingFingerprintServices = new Mock<IUsingFingerprintServices>(MockBehavior.Strict);
            modelService = new Mock<IModelService>(MockBehavior.Strict);
            audioService = new Mock<IAudioService>(MockBehavior.Strict);

            queryCommandBuilder = new QueryCommandBuilder(fingerprintCommandBuilder.Object, queryFingerprintService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            fingerprintCommandBuilder.VerifyAll();
            fingerprintingSource.VerifyAll();
            withAlgorithmConfiguration.VerifyAll();
            fingerprintCommand.VerifyAll();
            queryFingerprintService.VerifyAll();
        }

        [Test]
        public async Task QueryIsBuiltFromFileCorrectly()
        {
            const string pathToFile = "path-to-file";
            var dummyResult = new QueryResult(new List<ResultEntry>(), new QueryStats(0, 0, 0, 0));
            var hashedFingerprints =new Hashes(new List<HashedFingerprint>(
                    new[]
                        {
                            new HashedFingerprint(GenericHashBuckets(), 0, 0, Enumerable.Empty<string>()),
                            new HashedFingerprint(GenericHashBuckets(), 1, 0.928f, Enumerable.Empty<string>()),
                            new HashedFingerprint(GenericHashBuckets(), 2, 0.928f * 2, Enumerable.Empty<string>())
                        }), 0.928 * 3);

            fingerprintCommandBuilder.Setup(builder => builder.BuildFingerprintCommand()).Returns(fingerprintingSource.Object);
            fingerprintingSource.Setup(source => source.From(pathToFile)).Returns(withAlgorithmConfiguration.Object);
            withAlgorithmConfiguration.Setup(config => config.WithFingerprintConfig(It.IsAny<DefaultFingerprintConfiguration>())).Returns(usingFingerprintServices.Object);
            usingFingerprintServices.Setup(u => u.UsingServices(audioService.Object)).Returns(fingerprintCommand.Object);
            fingerprintCommand.Setup(command => command.Hash()).Returns(Task.Factory.StartNew(() => hashedFingerprints));
            queryFingerprintService.Setup(service => service.Query(hashedFingerprints, It.IsAny<DefaultQueryConfiguration>(), It.IsAny<DateTime>(), modelService.Object)).Returns(dummyResult);

            _ = await queryCommandBuilder.BuildQueryCommand()
                .From(pathToFile)
                .UsingServices(modelService.Object, audioService.Object)
                .Query();
        }

        [Test]
        public async Task QueryIsBuiltFromFileStartingAtAtSpecificSecondCorrectly()
        {
            const string pathToFile = "path-to-file";
            const int startAtSecond = 120;
            const int secondsToQuery = 20;
            QueryResult dummyResult = new QueryResult(new List<ResultEntry>(), new QueryStats(0, 0, 0, 0));
            var hashDatas = new Hashes(new List<HashedFingerprint>(
                    new[]
                        {
                            new HashedFingerprint(GenericHashBuckets(), 0, 0, Enumerable.Empty<string>()),
                            new HashedFingerprint(GenericHashBuckets(), 1, 0.928f, Enumerable.Empty<string>()),
                            new HashedFingerprint(GenericHashBuckets(), 2, 0.928f * 2, Enumerable.Empty<string>())
                        }), 0.928 * 3);
            fingerprintCommandBuilder.Setup(builder => builder.BuildFingerprintCommand()).Returns(fingerprintingSource.Object);
            fingerprintingSource.Setup(source => source.From(pathToFile, secondsToQuery, startAtSecond)).Returns(withAlgorithmConfiguration.Object);
            withAlgorithmConfiguration.Setup(config => config.WithFingerprintConfig(It.IsAny<DefaultFingerprintConfiguration>())).Returns(usingFingerprintServices.Object);
            usingFingerprintServices.Setup(u => u.UsingServices(audioService.Object)).Returns(fingerprintCommand.Object);
            fingerprintCommand.Setup(fingerprintingUnit => fingerprintingUnit.Hash()).Returns(Task.Factory.StartNew(() => hashDatas));
            queryFingerprintService.Setup(service => service.Query(hashDatas, It.IsAny<DefaultQueryConfiguration>(), It.IsAny<DateTime>(), modelService.Object)).Returns(dummyResult);

            _ = await queryCommandBuilder.BuildQueryCommand()
                                   .From(pathToFile, secondsToQuery, startAtSecond)
                                   .WithQueryConfig(
                                    config =>
                                       {
                                           config.FingerprintConfiguration.SpectrogramConfig.LogBase = 64;
                                           config.ThresholdVotes = 20;
                                           return config;
                                       })
                                   .UsingServices(modelService.Object, audioService.Object)
                                   .Query();

            fingerprintingSource.Verify(source => source.From(pathToFile, secondsToQuery, startAtSecond), Times.Once());
        }
   }
}
