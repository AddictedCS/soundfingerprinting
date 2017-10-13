namespace SoundFingerprinting.Tests.Unit.Builder
{
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
        private Mock<IWithFingerprintConfiguration> withAlgorithConfiguration;
        private Mock<IFingerprintCommand> fingerprintCommand;
        private Mock<IUsingFingerprintServices> usingFingerprintServices;
        private Mock<IModelService> modelService;
        private Mock<IAudioService> audioService;

        [SetUp]
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

        [TearDown]
        public void TearDown()
        {
            fingerprintCommandBuilder.VerifyAll();
            fingerprintingSource.VerifyAll();
            withAlgorithConfiguration.VerifyAll();
            fingerprintCommand.VerifyAll();
            queryFingerprintService.VerifyAll();
        }

        [Test]
        public void QueryIsBuiltFromFileCorrectly()
        {
            const string PathToFile = "path-to-file";
            QueryResult dummyResult = new QueryResult(new List<ResultEntry>());
            List<HashedFingerprint> hashedFingerprints =
                new List<HashedFingerprint>(
                    new[]
                        {
                            new HashedFingerprint(GenericSignature(), GenericHashBuckets(), 0, 0, Enumerable.Empty<string>()),
                            new HashedFingerprint(GenericSignature(), GenericHashBuckets(), 1, 0.928, Enumerable.Empty<string>()),
                            new HashedFingerprint(GenericSignature(), GenericHashBuckets(), 2, 0.928 * 2, Enumerable.Empty<string>())
                        });

            fingerprintCommandBuilder.Setup(builder => builder.BuildFingerprintCommand()).Returns(fingerprintingSource.Object);
            fingerprintingSource.Setup(source => source.From(PathToFile)).Returns(withAlgorithConfiguration.Object);
            withAlgorithConfiguration.Setup(config => config.WithFingerprintConfig(It.IsAny<EfficientFingerprintConfigurationForQuerying>())).Returns(usingFingerprintServices.Object);
            usingFingerprintServices.Setup(u => u.UsingServices(audioService.Object)).Returns(fingerprintCommand.Object);
            fingerprintCommand.Setup(command => command.Hash()).Returns(Task.Factory.StartNew(() => hashedFingerprints));
            queryFingerprintService.Setup(service => service.Query(hashedFingerprints, It.IsAny<DefaultQueryConfiguration>(), this.modelService.Object)).Returns(dummyResult);

            QueryResult queryResult = queryCommandBuilder.BuildQueryCommand()
                                   .From(PathToFile)
                                   .UsingServices(modelService.Object, audioService.Object)
                                   .Query()
                                   .Result;

            Assert.AreSame(dummyResult, queryResult);
        }

        [Test]
        public void QueryIsBuiltFromFileStartingAtAtSpecificSecondCorrectly()
        {
            const string PathToFile = "path-to-file";
            const int StartAtSecond = 120;
            const int SecondsToQuery = 20;
            QueryResult dummyResult = new QueryResult(new List<ResultEntry>());
            List<HashedFingerprint> hashDatas =
                new List<HashedFingerprint>(
                    new[]
                        {
                            new HashedFingerprint(GenericSignature(), GenericHashBuckets(), 0, 0, Enumerable.Empty<string>()),
                            new HashedFingerprint(GenericSignature(), GenericHashBuckets(), 1, 0.928, Enumerable.Empty<string>()),
                            new HashedFingerprint(GenericSignature(), GenericHashBuckets(), 2, 0.928 * 2, Enumerable.Empty<string>())
                        });
            fingerprintCommandBuilder.Setup(builder => builder.BuildFingerprintCommand()).Returns(fingerprintingSource.Object);
            fingerprintingSource.Setup(source => source.From(PathToFile, SecondsToQuery, StartAtSecond)).Returns(withAlgorithConfiguration.Object);
            withAlgorithConfiguration.Setup(config => config.WithFingerprintConfig(It.IsAny<DefaultFingerprintConfiguration>())).Returns(usingFingerprintServices.Object);
            usingFingerprintServices.Setup(u => u.UsingServices(audioService.Object)).Returns(fingerprintCommand.Object);
            fingerprintCommand.Setup(fingerprintingUnit => fingerprintingUnit.Hash()).Returns(Task.Factory.StartNew(() => hashDatas));
            queryFingerprintService.Setup(service => service.Query(hashDatas, It.IsAny<DefaultQueryConfiguration>(), this.modelService.Object)).Returns(dummyResult);

            QueryResult queryResult = queryCommandBuilder.BuildQueryCommand()
                                   .From(PathToFile, SecondsToQuery, StartAtSecond)
                                   .WithConfigs(
                                    config =>
                                       {
                                           config.SpectrogramConfig.LogBase = 64;
                                           return config;
                                       },
                                    config =>
                                       {
                                           config.ThresholdVotes = 20;
                                           return config;
                                       })
                                   .UsingServices(modelService.Object, audioService.Object)
                                   .Query()
                                   .Result;

            Assert.AreSame(dummyResult, queryResult);
            fingerprintingSource.Verify(source => source.From(PathToFile, SecondsToQuery, StartAtSecond), Times.Once());
        }

        [Test]
        public void QueryCommandIsBuiltWithDefaultConfigsCorrectly()
        {
            var command = queryCommandBuilder.BuildQueryCommand()
                               .From("path-to-file", 10, 0)
                               .UsingServices(modelService.Object, audioService.Object);

            Assert.IsInstanceOf<EfficientFingerprintConfigurationForQuerying>(command.FingerprintConfiguration);
            Assert.IsInstanceOf<DefaultQueryConfiguration>(command.QueryConfiguration);
        }

        [Test]
        public void QueryCommandIsBuiltWithCustomConfigsCorrectly()
        {
            var command = queryCommandBuilder.BuildQueryCommand()
                                             .From("path-to-file", 10, 0)
                                             .WithConfigs(
                                                 config =>
                                                     {
                                                         config.SpectrogramConfig.ImageLength = 1024;
                                                         return config;
                                                     },
                                                 config => 
                                                     {
                                                         config.ThresholdVotes = 256;
                                                         return config;
                                                     })
                                             .UsingServices(modelService.Object, audioService.Object);

            Assert.AreEqual(1024, command.FingerprintConfiguration.SpectrogramConfig.ImageLength);
            Assert.AreEqual(256, command.QueryConfiguration.ThresholdVotes);
        }

        [Test]
        public void QueryCommandIsBuiltWithCustomFingerprintConfigCorrectly()
        {
            var customConfig = new DefaultFingerprintConfiguration();
            var command = queryCommandBuilder.BuildQueryCommand()
                                             .From("path-to-file", 10, 0)
                                             .WithFingerprintConfig(customConfig)
                                             .UsingServices(modelService.Object, audioService.Object);

            Assert.AreSame(command.FingerprintConfiguration, customConfig);
        }

        [Test]
        public void QueryCommandIsBuiltWithCustomQueryConfigCorrectly()
        {
            var customConfig = new DefaultQueryConfiguration();

            var command = queryCommandBuilder.BuildQueryCommand()
                                             .From("path-to-file")
                                             .WithQueryConfig(customConfig)
                                             .UsingServices(modelService.Object, audioService.Object);

            Assert.AreSame(command.QueryConfiguration, customConfig);
        }

        [Test]
        public void QueryCommandIsBuiltWithCustomQueryAndFingerprintConfigCorrectly()
        {
            var customQueryConfig = new DefaultQueryConfiguration();
            var customFingerprintConfig = new DefaultFingerprintConfiguration();

            var command = queryCommandBuilder.BuildQueryCommand()
                                             .From("path-to-file")
                                             .WithConfigs(customFingerprintConfig, customQueryConfig)
                                             .UsingServices(modelService.Object, audioService.Object);

            Assert.AreSame(command.QueryConfiguration, customQueryConfig);
            Assert.AreSame(command.FingerprintConfiguration, customFingerprintConfig);
        }

        [Test]
        public void QueryCommandIsBuildWithQueryConfigAmmender()
        {
            var command = queryCommandBuilder
                .BuildQueryCommand()
                .From("path-to-audio-file")
                .WithQueryConfig(
                    config =>
                    {
                        config.Clusters = new[] { "CA", "WA" };
                        return config;
                    })
                .UsingServices(modelService.Object, audioService.Object);

            CollectionAssert.AreEqual(new[] { "CA", "WA" }, command.QueryConfiguration.Clusters);
        }
    }
}
