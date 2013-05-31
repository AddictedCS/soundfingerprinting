namespace Soundfingerprinting.DuplicatesDetector.Infrastructure
{
    using System.Diagnostics;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    ///   Class for writing any object values in comma separated file
    /// </summary>
    [DebuggerDisplay("Path={pathToFile}")]
    public class CSVWriter
    {
        /// <summary>
        ///   Separator used while writing to CVS
        /// </summary>
        private const char Separator = ',';

        /// <summary>
        ///   Carriage return line feed
        /// </summary>
        private const string Crlf = "\r\n";

        /// <summary>
        ///   Path to file
        /// </summary>
        private readonly string pathToFile;

        /// <summary>
        ///   Writer
        /// </summary>
        private StreamWriter writer;

        public CSVWriter(string pathToFile)
        {
            this.pathToFile = pathToFile;
        }

        /// <summary>
        ///   Write the data into CSV
        /// </summary>
        /// <param name = "data">Data to be written</param>
        [FileIOPermission(SecurityAction.Demand)]
        public void Write(object[][] data)
        {
            if (data == null)
            {
                return;
            }

            using (writer = new StreamWriter(pathToFile))
            {
                int cols = data[0].Length;
                StringBuilder builder = new StringBuilder();
                for (int i = 0, n = data.Length; i < n; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        builder.Append(data[i][j]);
                        if (j != cols - 1)
                        {
                            builder.Append(Separator);
                        }
                    }

                    builder.Append(Crlf);
                }

                writer.Write(builder.ToString());
                writer.Close();
            }
        }
    }
}