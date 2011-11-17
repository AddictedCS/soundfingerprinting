// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.DbStorage;
using Soundfingerprinting.Fingerprinting;

namespace Soundfingerprinting.UnitTests.Fingerprinting.Tests.Hash
{
    /// <summary>
    ///   Testing Min Hash class
    /// </summary>
    [TestClass]
    public class MinHashTest : BaseTest
    {
        private readonly string _connectionstring;

        /// <summary>
        ///   Dal Fingerprint manager object
        /// </summary>
        private readonly DaoGateway _dalManager;


        /// <summary>
        ///   Fingerprint manager
        /// </summary>
        private readonly FingerprintManager _fingerManager = new FingerprintManager();

        /// <summary>
        ///   Start index
        /// </summary>
        private int _startIndex = -1;

        public MinHashTest()
        {
            _connectionstring = ConnectionString;
            _dalManager = new DaoGateway(_connectionstring);
        }
    }
}