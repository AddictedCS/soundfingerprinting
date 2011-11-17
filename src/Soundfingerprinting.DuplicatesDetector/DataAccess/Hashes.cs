// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Collections.Generic;
using Soundfingerprinting.DuplicatesDetector.Model;

namespace Soundfingerprinting.DuplicatesDetector.DataAccess
{
    /// <summary>
    ///   Hashes used in Query/Creation mechanisms
    /// </summary>
    public class Hashes
    {
        /// <summary>
        ///   Parameter less constructor
        /// </summary>
        public Hashes()
        {
            Query = new HashSet<HashSignature>();
            Creational = new HashSet<HashSignature>();
        }

        /// <summary>
        ///   Query hashes (used in querying the database)
        /// </summary>
        public HashSet<HashSignature> Query { get; set; }

        /// <summary>
        ///   Creational hashes (used in creation of the database)
        /// </summary>
        public HashSet<HashSignature> Creational { get; set; }
    }
}