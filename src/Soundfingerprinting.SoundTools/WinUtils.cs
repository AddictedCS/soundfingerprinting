// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Soundfingerprinting.SoundTools.Properties;

namespace Soundfingerprinting.SoundTools
{
    using Soundfingerprinting.Audio.Strides;

    internal static class WinUtils
    {
        /// <summary>
        ///   Gets path to all files specified by the filter
        /// </summary>
        /// <param name = "filters">Filter (*.mp3)</param>
        /// <param name = "rootFolder">Root folder to start searching</param>
        /// <param name = "includeSubdirectories">Include subdirectories (true/false)</param>
        /// <returns>List of all available files</returns>
        public static List<string> GetPathToAllFiles(IEnumerable<string> filters, string rootFolder, bool includeSubdirectories)
        {
            if (String.IsNullOrEmpty(rootFolder))
                return null;
            List<string> fileList = new List<string>();
            try
            {
                fileList.AddRange(filters.SelectMany(filter => Directory.GetFiles(rootFolder, filter, (includeSubdirectories) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)));
            }
            catch
            {
                return null;
            }
            return fileList;
        }

        /// <summary>
        ///   Get multiple filter for open file dialog
        /// </summary>
        /// <param name = "caption">Caption</param>
        /// <param name = "filters">List of filters</param>
        /// <returns>Multiple filter</returns>
        public static string GetMultipleFilter(string caption, IEnumerable<string> filters)
        {
            StringBuilder filter = new StringBuilder(caption);
            filter.Append(" (");
            for (int i = 0; i < filters.Count(); i++)
            {
                filter.Append(filters.ElementAt(i));
                if (i != filters.Count() - 1 /*last*/)
                    filter.Append(";");
                else
                {
                    filter.Append(")|");
                    for (int j = 0; j < filters.Count(); j++)
                    {
                        filter.Append(filters.ElementAt(j));
                        if (j != filters.Count() - 1 /*last*/)
                            filter.Append(";");
                    }
                }
            }
            return filter.ToString();
        }

        /// <summary>
        ///   Export in Excel the results from any datagridview
        /// </summary>
        /// <param name = "dgvResults">DataGridView with the results</param>
        /// <param name = "footer">Footer written at the end of the file</param>
        public static void ExportInExcel(DataGridView dgvResults, params Object[] footer)
        {
            SaveFileDialog ofd = new SaveFileDialog {Filter = Resources.FileFilterCSV, FileName = "results.csv"};
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                CSVWriter writer = new CSVWriter(ofd.FileName);

                object[][] values = new object[dgvResults.Rows.Count + 2][];
                int rowIndex = 0;
                int colIndex = 0;
                foreach (DataGridViewColumn col in dgvResults.Columns) /*Writing Column Headers*/
                {
                    values[colIndex] = new object[dgvResults.Columns.Count];
                    values[rowIndex][colIndex] = col.HeaderText;
                    colIndex++;
                }
                rowIndex++; /*1*/

                foreach (DataGridViewRow row in dgvResults.Rows) /*Writing the values*/
                {
                    colIndex = 0;
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        values[rowIndex][colIndex] = cell.Value;
                        colIndex++;
                    }
                    rowIndex++;
                }

                for (int i = 0; i < footer.Length; i++)
                    values[rowIndex][i] = footer[i];

                /*Writing the results in the last row*/
                writer.Write(values);
            }
        }

        /// <summary>
        ///   Get list of path to files according to the filters and root folder
        /// </summary>
        /// <param name = "filters">Filters</param>
        /// <param name = "rootFolder">Start folder</param>
        /// <returns>Path to files</returns>
        public static List<string> GetFiles(IEnumerable<string> filters, string rootFolder)
        {
            List<string> fileList = null;
            if (Directory.Exists(rootFolder)) /*If such path exists*/
            {
                fileList = GetPathToAllFiles(filters, rootFolder, true);
            }
            return fileList;
        }

        /// <summary>
        ///   Get stride value
        /// </summary>
        /// <param name = "type">Type of the stride</param>
        /// <param name = "maxStride">Maximum stride value</param>
        /// <param name = "minStride">Minimum stride value</param>
        /// <param name = "samplesPerFingerprint">Samples per signature</param>
        /// <returns>Stride object</returns>
        public static IStride GetStride(StrideType type, int maxStride, int minStride, int samplesPerFingerprint)
        {
            switch (type)
            {
                case StrideType.Static:
                    return new StaticStride(maxStride);
                case StrideType.Random:
                    return new RandomStride(minStride, maxStride);
                case StrideType.IncrementalStatic:
                    return new IncrementalStaticStride(maxStride, samplesPerFingerprint);
                case StrideType.IncrementalRandom:
                    return new IncrementalRandomStride(minStride, maxStride, samplesPerFingerprint);
                default:
                    throw new ArgumentException("Cannot find a matching type");
            }
        }
    }
}