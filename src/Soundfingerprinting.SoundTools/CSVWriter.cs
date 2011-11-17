// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Text;

namespace Soundfingerprinting.SoundTools
{
    /// <summary>
    ///   Class for writing any object values in comma separated file
    /// </summary>
    [DebuggerDisplay("Path={_pathToFile}")]
    public class CSVWriter
    {
        /// <summary>
        ///   Separator used while writing to CVS
        /// </summary>
        private const char SEPARATOR = ',';

        /// <summary>
        ///   Carriage return line feed
        /// </summary>
        private const string CRLF = "\r\n";

        /// <summary>
        ///   Path to file
        /// </summary>
        private readonly string _pathToFile;

        /// <summary>
        ///   Writer
        /// </summary>
        private StreamWriter _writer;

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "pathToFile">Path to filename</param>
        public CSVWriter(string pathToFile)
        {
            _pathToFile = pathToFile;
        }

        /// <summary>
        ///   Write the data into CSV
        /// </summary>
        /// <param name = "data">Data to be written</param>
        [FileIOPermission(SecurityAction.Demand)]
        public void Write(object[][] data)
        {
            using (_writer = new StreamWriter(_pathToFile))
            {
                int cols = data[0].Length;
                StringBuilder builder = new StringBuilder();
                for (int i = 0, n = data.GetLength(0); i < n; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        builder.Append(data[i][j]);
                        if (j != cols - 1)
                            builder.Append(SEPARATOR);
                    }
                    builder.Append(CRLF);
                }
                _writer.Write(builder.ToString());
                _writer.Close();
            }
        }
    }
}