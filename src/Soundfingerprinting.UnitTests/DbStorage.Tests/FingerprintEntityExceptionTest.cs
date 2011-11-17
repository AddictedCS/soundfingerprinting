// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.DbStorage;

namespace Soundfingerprinting.UnitTests.DbStorage.Tests
{
    ///<summary>
    ///  This is a test class for FingerprintEntityExceptionTest and is intended
    ///  to contain all FingerprintEntityExceptionTest Unit Tests
    ///</summary>
    [TestClass]
    public class FingerprintEntityExceptionTest : BaseTest
    {
        ///<summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        ///<summary>
        ///  A test for FingerprintEntityException Constructor
        ///</summary>
        [TestMethod]
        public void FingerprintEntityExceptionConstructorTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            string message = name;
            Exception innerException = null;
            FingerprintEntityException target = new FingerprintEntityException(message, innerException);
            Assert.AreEqual(message, target.Message);
            Assert.AreEqual(innerException, target.InnerException);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for FingerprintEntityException Constructor
        ///</summary>
        [TestMethod]
        public void FingerprintEntityExceptionConstructorTest1()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            FingerprintEntityException target = new FingerprintEntityException();
            Assert.IsTrue(true);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        ///<summary>
        ///  A test for FingerprintEntityException Constructor
        ///</summary>
        [TestMethod]
        public void FingerprintEntityExceptionConstructorTest2()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            string message = name;
            FingerprintEntityException target = new FingerprintEntityException(message);
            Assert.AreEqual(message, target.Message);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}