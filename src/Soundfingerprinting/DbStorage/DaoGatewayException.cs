// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Runtime.Serialization;

namespace Soundfingerprinting.DbStorage
{
    /// <summary>
    ///   Data Access Layer Exception
    /// </summary>
    [Serializable]
    public class DalGatewayException : Exception
    {
        #region Constructors

        /// <summary>
        ///   Default constructor
        /// </summary>
        public DalGatewayException()
        {
        }

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "message">Exception message</param>
        public DalGatewayException(string message) : base(message)
        {
        }

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "message">Exception message</param>
        /// <param name = "innerException">Inner exception</param>
        public DalGatewayException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "info">Serialization info</param>
        /// <param name = "ctx">Serialization context</param>
        protected DalGatewayException(SerializationInfo info, StreamingContext ctx) : base(info, ctx)
        {
        }

        #endregion
    }
}