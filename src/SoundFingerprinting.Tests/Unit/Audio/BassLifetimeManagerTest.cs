namespace SoundFingerprinting.Tests.Unit.Audio
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Audio.Bass;
    using SoundFingerprinting.Infrastructure;

    using Un4seen.Bass;

    [TestClass]
    public class BassLifetimeManagerTest : AbstractTest
    {
        private BassServiceProxy.BassLifetimeManager bassLifetimeManager;

        private Mock<IBassServiceProxy> bassServiceProxy;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            DependencyResolver.Current.Get<IBassServiceProxy>().Dispose();
        }

        [TestInitialize]
        public void SetUp()
        {
            bassServiceProxy = new Mock<IBassServiceProxy>(MockBehavior.Strict);
        }

        [TestCleanup]
        public void TearDown()
        {
            bassServiceProxy.VerifyAll();
            if (bassLifetimeManager != null)
            {
                RegisterAllCleanupCalls();

                bassLifetimeManager.Dispose();
            }
        }

        [TestMethod]
        public void TestBassLibraryIsInitializedOnlyOnce()
        {
            RegisterAllInitializationCalls();

            bassLifetimeManager = new BassServiceProxy.BassLifetimeManager(bassServiceProxy.Object);

            Assert.IsNotNull(bassLifetimeManager);
        }

        [TestMethod]
        public void TestBassLibraryIsReleaseOnlyOnce()
        {
            RegisterAllInitializationCalls();

            RegisterAllCleanupCalls();

            using (var manager = new BassServiceProxy.BassLifetimeManager(bassServiceProxy.Object))
            {
                Assert.IsNotNull(manager);
            }

            bassServiceProxy.Verify(proxy => proxy.BassFree(), Times.Once);
            bassServiceProxy.Verify(proxy => proxy.PluginFree(0), Times.Once);
        }

        [TestMethod]
        public void TestBassLibraryIsInitializedThenReleasedThenInitializedOneMoreTime()
        {
            RegisterAllInitializationCalls();

            RegisterAllCleanupCalls();

            using (var manager = new BassServiceProxy.BassLifetimeManager(bassServiceProxy.Object))
            {
                Assert.IsNotNull(manager);
            }

            using (var newManager = new BassServiceProxy.BassLifetimeManager(bassServiceProxy.Object))
            {
                Assert.IsNotNull(newManager);
            }

            bassServiceProxy.Verify(proxy => proxy.BassFree(), Times.Exactly(2));
            bassServiceProxy.Verify(proxy => proxy.PluginFree(0), Times.Exactly(2));
        }

        private void RegisterAllInitializationCalls()
        {
            const string RegistrationEmail = "gleb.godonoga@gmail.com";
            const string RegistrationKey = "2X155323152222";

            bassServiceProxy.Setup(proxy => proxy.RegisterBass(RegistrationEmail, RegistrationKey));
            bassServiceProxy.Setup(proxy => proxy.BassLoadMe(It.IsAny<string>())).Returns(true);
            bassServiceProxy.Setup(proxy => proxy.BassMixLoadMe(It.IsAny<string>())).Returns(true);
            bassServiceProxy.Setup(proxy => proxy.BassFxLoadMe(It.IsAny<string>())).Returns(true);
            bassServiceProxy.Setup(proxy => proxy.GetVersion()).Returns(1232);
            bassServiceProxy.Setup(proxy => proxy.GetMixerVersion()).Returns(1233);
            bassServiceProxy.Setup(proxy => proxy.GetFxVersion()).Returns(1234);
            bassServiceProxy.Setup(proxy => proxy.PluginLoadDirectory(It.IsAny<string>())).Returns(
                new Dictionary<int, string> { { 1, "bassflac.dll" } });
            bassServiceProxy.Setup(
                proxy =>
                proxy.Init(-1, SampleRate, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_MONO))
                .Returns(true);
            bassServiceProxy.Setup(proxy => proxy.SetConfig(BASSConfig.BASS_CONFIG_MIXER_FILTER, 50)).Returns(true);
            bassServiceProxy.Setup(proxy => proxy.SetConfig(BASSConfig.BASS_CONFIG_FLOATDSP, true)).Returns(true);
            bassServiceProxy.Setup(proxy => proxy.RecordInit(-1)).Returns(true);
        }

        private void RegisterAllCleanupCalls()
        {
            bassServiceProxy.Setup(proxy => proxy.BassFree()).Returns(true);
            bassServiceProxy.Setup(proxy => proxy.PluginFree(0)).Returns(true);
        }
    }
}
