namespace Soundfingerprinting.DuplicatesDetector.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Text;

    public static class Helper
    {
        /// <summary>
        ///   Count the number of music files in the a specific folder and it's subdirectories
        /// </summary>
        /// <param name = "path">Path of the root folder</param>
        /// <param name = "filters">Search filters</param>
        /// <param name = "includeSubdirectories">Include subdirectories in the search</param>
        /// <returns>Number of music files</returns>
        public static int CountNumberOfMusicFiles(string path, IEnumerable<string> filters, bool includeSubdirectories)
        {
            int files = 0;
            DirectoryInfo root = new DirectoryInfo(path);
            var listOfFilters = filters as IList<string> ?? filters.ToList();
            foreach (string filter in listOfFilters)
            {
                try
                {
                    foreach (FileInfo file in root.GetFiles(filter, SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            string temp = file.FullName; /*Dummy call to see if path does not exceed the limit*/
                        }
                        catch (PathTooLongException)
                        {
                            continue;
                        }

                        files++;
                    }
                }
                catch (SecurityException)
                {
                    /*you don't have the permission of viewing this files*/
                    files += 0;
                }
                catch (DirectoryNotFoundException)
                {
                    /*directory not found*/
                    files += 0;
                }
                catch (ArgumentNullException)
                {
                    /*bad parameters*/
                    files += 0;
                }
            }

            if (includeSubdirectories)
            {
                DirectoryInfo[] directories;
                try
                {
                    directories = root.GetDirectories();
                }
                catch (DirectoryNotFoundException)
                {
                    /*directory wasn't found*/
                    return files;
                }

                foreach (DirectoryInfo directory in directories)
                {
                    try
                    {
                        files += CountNumberOfMusicFiles(directory.FullName, listOfFilters, true);
                    }
                    catch (PathTooLongException)
                    {
                        // swalow
                    }
                }
            }

            return files;
        }

        /// <summary>
        ///   Get music files from root folder and it's subdirectories
        /// </summary>
        /// <param name = "path">Path of the root folder</param>
        /// <param name = "filters">Search filters</param>
        /// <param name = "includeSubdirectories">Include subdirectories in the search</param>
        /// <returns>List with the music files</returns>
        public static List<string> GetMusicFiles(string path, IEnumerable<string> filters, bool includeSubdirectories)
        {
            List<string> files = new List<string>();
            DirectoryInfo root = new DirectoryInfo(path);
            var listOfFilters = filters as IList<string> ?? filters.ToList();
            foreach (string filter in listOfFilters)
            {
                try
                {
                    foreach (FileInfo file in root.GetFiles(filter, SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            files.Add(file.FullName);
                        }
                        catch (PathTooLongException)
                        {
                            /*255 item range is too long*/
                        }
                    }
                }
                catch (SecurityException)
                {
                    /*you don't have the permission of viewing this files*/
                }
                catch (DirectoryNotFoundException)
                {
                    /*directory not found*/
                }
                catch (ArgumentNullException)
                {
                    /*bad parameters*/
                }
            }

            if (includeSubdirectories)
            {
                DirectoryInfo[] directories;
                try
                {
                    directories = root.GetDirectories();
                }
                catch (DirectoryNotFoundException)
                {
                    /*directory wasn't found*/
                    return files;
                }

                foreach (DirectoryInfo directory in directories)
                {
                    try
                    {
                        files.AddRange(GetMusicFiles(directory.FullName, listOfFilters, true));
                    }
                    catch (PathTooLongException)
                    {
                        // continue
                    }
                }
            }

            return files;
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
            var listOfFilters = filters as IList<string> ?? filters.ToList();
            for (int i = 0; i < listOfFilters.Count(); i++)
            {
                filter.Append(listOfFilters.ElementAt(i));
                if (i != listOfFilters.Count - 1 /*last*/)
                {
                    filter.Append(";");
                }
                else
                {
                    filter.Append(")|");
                    for (int j = 0; j < listOfFilters.Count(); j++)
                    {
                        filter.Append(listOfFilters.ElementAt(j));
                        if (j != listOfFilters.Count - 1 /*last*/)
                        {
                            filter.Append(";");
                        }
                    }
                }
            }

            return filter.ToString();
        }
    }
}