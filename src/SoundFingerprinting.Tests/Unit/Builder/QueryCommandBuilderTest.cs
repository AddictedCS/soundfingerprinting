namespace SoundFingerprinting.Tests.Unit.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;

    using NUnit.Framework;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Command;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Content;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.InMemory;
    using SoundFingerprinting.Media;
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

            queryCommandBuilder = new QueryCommandBuilder(fingerprintCommandBuilder.Object, queryFingerprintService.Object, new NullLoggerFactory());
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
        public void ShouldThrowOnMissingServices()
        {
            Assert.ThrowsAsync<ArgumentException>(() => QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From("test.mp4", MediaType.Audio | MediaType.Video)
                .UsingServices(new InMemoryModelService())
                .Query());
            
            Assert.ThrowsAsync<ArgumentException>(() => QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From(TestUtilities.GenerateRandomAudioSamples(100))
                .Query());

            Assert.ThrowsAsync<ArgumentException>(() => QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From("test.mp4", MediaType.Video)
                .UsingServices(new InMemoryModelService(), new SoundFingerprintingAudioService())
                .Query());
        }

        [Test]
        public async Task ShouldQueryUsingAudioVideoHashes()
        {
            var modelService = new Mock<IModelService>();
            var mediaService = new Mock<IMediaService>();

            var avTrack = new AVTrack(new AudioTrack(TestUtilities.GenerateRandomAudioSamples(30 * 5512)), new VideoTrack(TestUtilities.GenerateRandomFrames(30 * 30)));
            mediaService.Setup(_ => _.ReadAVTrackFromFile("test.mp4", It.IsAny<AVTrackReadConfiguration>(), 0, 0, MediaType.Audio | MediaType.Video)).Returns(avTrack);
            modelService.Setup(_ => _.Query(It.IsAny<Hashes>(), It.IsAny<QueryConfiguration>())).Callback(
                (Hashes hashes, QueryConfiguration configuration) =>
                {
                    Assert.AreEqual(30, hashes.DurationInSeconds, 0.001);
                }).Returns(Enumerable.Empty<SubFingerprintData>());
            
            var avQueryResult = await QueryCommandBuilder.Instance
                .BuildQueryCommand()
                .From("test.mp4", MediaType.Audio | MediaType.Video)
                .UsingServices(modelService.Object, mediaService.Object)
                .Query();
            
            Assert.IsFalse(avQueryResult.ContainsMatches);
            
            modelService.Verify(_ => _.Query(It.IsAny<Hashes>(), It.IsAny<QueryConfiguration>()), Times.Exactly(2));
        }
        
        [Test]
        public async Task QueryIsBuiltFromFileCorrectly()
        {
            const string pathToFile = "path-to-file";
            var dummyResult = new QueryResult(new List<ResultEntry>(), Hashes.GetEmpty(MediaType.Audio), new QueryCommandStats(0, 0, 0, 0));
            var hashedFingerprints = new AVHashes(new Hashes(new List<HashedFingerprint>(
                    new[]
                        {
                            new HashedFingerprint(GenericHashBuckets(), 0, 0, Array.Empty<byte>()),
                            new HashedFingerprint(GenericHashBuckets(), 1, 0.928f, Array.Empty<byte>()),
                            new HashedFingerprint(GenericHashBuckets(), 2, 0.928f * 2, Array.Empty<byte>())
                        }), 0.928 * 3, MediaType.Audio), null);

            fingerprintCommandBuilder.Setup(builder => builder.BuildFingerprintCommand()).Returns(fingerprintingSource.Object);
            fingerprintingSource.Setup(source => source.From(pathToFile, MediaType.Audio)).Returns(withAlgorithmConfiguration.Object);
            withAlgorithmConfiguration.Setup(config => config.WithFingerprintConfig(It.IsAny<DefaultAVFingerprintConfiguration>())).Returns(usingFingerprintServices.Object);
            usingFingerprintServices.Setup(u => u.UsingServices(audioService.Object)).Returns(fingerprintCommand.Object);
            fingerprintCommand.Setup(command => command.Hash()).Returns(Task.Factory.StartNew(() => hashedFingerprints));
            queryFingerprintService.Setup(service => service.Query(hashedFingerprints.Audio, It.IsAny<DefaultQueryConfiguration>(), modelService.Object)).Returns(dummyResult);

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
            var dummyResult = new QueryResult(new List<ResultEntry>(), Hashes.GetEmpty(MediaType.Audio), new QueryCommandStats(0, 0, 0, 0));
            var hashes = new AVHashes(new Hashes(new List<HashedFingerprint>(
                    new[]
                        {
                            new HashedFingerprint(GenericHashBuckets(), 0, 0, Array.Empty<byte>()),
                            new HashedFingerprint(GenericHashBuckets(), 1, 0.928f, Array.Empty<byte>()),
                            new HashedFingerprint(GenericHashBuckets(), 2, 0.928f * 2, Array.Empty<byte>())
                        }), 0.928 * 3, MediaType.Audio), null);
            fingerprintCommandBuilder.Setup(builder => builder.BuildFingerprintCommand()).Returns(fingerprintingSource.Object);
            fingerprintingSource.Setup(source => source.From(pathToFile, secondsToQuery, startAtSecond, MediaType.Audio)).Returns(withAlgorithmConfiguration.Object);
            withAlgorithmConfiguration.Setup(config => config.WithFingerprintConfig(It.IsAny<DefaultAVFingerprintConfiguration>())).Returns(usingFingerprintServices.Object);
            usingFingerprintServices.Setup(u => u.UsingServices(audioService.Object)).Returns(fingerprintCommand.Object);
            fingerprintCommand.Setup(fingerprintingUnit => fingerprintingUnit.Hash()).Returns(Task.Factory.StartNew(() => hashes));
            queryFingerprintService.Setup(service => service.Query(hashes.Audio, It.IsAny<DefaultQueryConfiguration>(), modelService.Object)).Returns(dummyResult);

            _ = await queryCommandBuilder.BuildQueryCommand()
                                   .From(pathToFile, secondsToQuery, startAtSecond)
                                   .WithQueryConfig(
                                    config =>
                                       {
                                           config.FingerprintConfiguration.Audio.SpectrogramConfig.LogBase = 64;
                                           config.Audio.ThresholdVotes = 20;
                                           return config;
                                       })
                                   .UsingServices(modelService.Object, audioService.Object)
                                   .Query();

            fingerprintingSource.Verify(source => source.From(pathToFile, secondsToQuery, startAtSecond, MediaType.Audio), Times.Once());
        }
   }
}
