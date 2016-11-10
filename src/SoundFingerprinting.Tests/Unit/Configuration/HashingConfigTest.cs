namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Configuration;

    [TestClass]
    public class HashingConfigTest
    {
        [TestMethod]
        public void CustomHashValuesAreInheritedFromDefault()
        {
            HashingConfig defaultConfig = new DefaultHashingConfig();
            HashingConfig customConfig = new CustomHashingConfig();

            Assert.AreEqual(defaultConfig.NumberOfLSHTables, customConfig.NumberOfLSHTables);
            Assert.AreEqual(defaultConfig.NumberOfMinHashesPerTable, customConfig.NumberOfMinHashesPerTable);
        }

        [TestMethod]
        public void DefaultValuesAreOverwrittenByCustom()
        {
            HashingConfig customConfig = new CustomHashingConfig { NumberOfLSHTables = 20, NumberOfMinHashesPerTable = 5 };

            Assert.AreEqual(20, customConfig.NumberOfLSHTables);
            Assert.AreEqual(5, customConfig.NumberOfMinHashesPerTable);
        }
    }
}
