using System;
using System.Diagnostics;
using System.Reflection;
using SoundfingerprintingLib.DbStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using SoundfingerprintingLib.DbStorage.ConnectionManager;

namespace SoundfingerprintingLib.Tests.DbStorage.Tests
{
    
    /// <summary>
    ///This is a test class for MSDaoConnectionTest and is intended
    ///to contain all MSDaoConnectionTest Unit Tests
    ///</summary>
    [TestClass()]
// ReSharper disable InconsistentNaming
    public class MSDaoConnectionTest : BaseTest
// ReSharper restore InconsistentNaming
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///A test for GetConnection
        ///</summary>
        [TestMethod()]
        public void GetConnectionTest()
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            MSDaoConnection target = new MSDaoConnection(); 
            string connectionString = ConnectionString; 
            IDbConnection expected = target.GetConnection(ConnectionString); 
            IDbConnection actual = target.GetConnection(connectionString);
            Assert.AreNotEqual(expected, actual);
            
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }

        /// <summary>
        ///A test for MSDaoConnection Constructor
        ///</summary>
        [TestMethod()]
// ReSharper disable InconsistentNaming
        public void MSDaoConnectionConstructorTest()
// ReSharper restore InconsistentNaming
        {
            string name = MethodBase.GetCurrentMethod().Name;
            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test started at : " + DateTime.Now);

            MSDaoConnection target = new MSDaoConnection();
            Assert.IsNotNull(target.GetConnection(ConnectionString));

            if (Bswitch.Enabled)
                Trace.WriteLine("#" + name + " : test ended at : " + DateTime.Now);
        }
    }
}
