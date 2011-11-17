// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.DbStorage;

namespace Soundfingerprinting.UnitTests.NeuralHashing.Tests
{
    [TestClass]
    public class NetTrainerTest : BaseTest
    {
        private readonly DaoGateway _dalManager;

        public NetTrainerTest()
        {
            _dalManager = new DaoGateway(ConnectionString);
        }
    }
}