namespace MCBE_ServerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// General utilities.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Determines whether or not this process is running on Linux.
        /// </summary>
        /// <returns>True if running on Linux, otherwise false.</returns>
        public static bool IsLinux()
        {
            var p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }

        /// <summary>
        /// Determines if the required server files are present.
        /// </summary>
        /// <param name="rootDirectory">Expected root directory of the server files.</param>
        /// <returns>True if expected files are present, otherwise false.</returns>
        public static bool ValidateServerFiles(string rootDirectory)
        {
            var requiredFiles = new List<string>();

            if (IsLinux())
            {
                requiredFiles.Add("bedrock_server");
                requiredFiles.Add("libCrypto.so");
            }
            else
            {
                requiredFiles.Add("bedrock_server.exe");
            }

            foreach (var file in requiredFiles)
            {
                if (!File.Exists(file))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the version of this program.
        /// </summary>
        public static string ProgramVersion => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
    }
}
