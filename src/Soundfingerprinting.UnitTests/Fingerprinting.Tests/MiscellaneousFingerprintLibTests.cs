// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.DbStorage;
using Soundfingerprinting.Fingerprinting;

namespace Soundfingerprinting.UnitTests.Fingerprinting.Tests
{
    [TestClass]
    public class MiscellaneousDspTests : BaseTest
    {
        /// <summary>
        ///   Constructor test
        /// </summary>
        [TestMethod]
        public void DalExceptionConstructorsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Exception inner = new Exception("InnerException");
            DalGatewayException ex = new DalGatewayException();
            ex = new DalGatewayException(name);
            ex = new DalGatewayException(name, inner);
            Assert.AreEqual(name, ex.Message);
            Assert.AreEqual("InnerException", ex.InnerException.Message);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///   Constructor test
        /// </summary>
        [TestMethod]
        public void FingerprintManagerEventArgsTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            Exception ex = new Exception("#FingerprintManagerEventArgsTest");
            FingerprintManagerEventArgs args = new FingerprintManagerEventArgs(ex);
            Assert.AreEqual(args.UnhandledException, ex);

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}