﻿namespace AhlSoft.BedrockServerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using AhlSoft.BedrockServerWrapper.Logging;

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
            return p == 4 || p == 6 || p == 128;
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
                requiredFiles.Add(Path.Combine(rootDirectory, "bedrock_server"));
                requiredFiles.Add(Path.Combine(rootDirectory, "libCrypto.so"));
            }
            else
            {
                requiredFiles.Add(Path.Combine(rootDirectory, "bedrock_server.exe"));
            }

            return requiredFiles.All(File.Exists);
        }

        /// <summary>
        /// Converts a given number of minutes to a friendly format.
        /// </summary>
        /// <param name="minutes">Number of minutes to convert.</param>
        /// <returns>A formatted string representing the given number of minutes.</returns>
        public static string TimePlayedConversion(int minutes)
        {
            var timespan = TimeSpan.FromMinutes(minutes);

            return $"{Math.Floor(timespan.TotalHours):00}h {timespan.Minutes:00}m";
        }

        /// <summary>
        /// Gets the version of this program.
        /// </summary>
        public static string ProgramVersion => FileVersionInfo.GetVersionInfo(Environment.GetCommandLineArgs()[0]).FileVersion;

        /// <summary>
        /// Attempt to delete a directory.
        /// </summary>
        /// <param name="dir">Path to directory to delete.</param>
        /// <param name="log">Logger to use.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool DeleteDirectory(string dir, ILog log)
        {
	        try
	        {
		        Directory.Delete(dir, true);
		        return true;
	        }
	        catch (Exception e)
	        {
		        log?.Error($"Couldn't delete files/folder: {e.GetType()} - {e.Message}");
		        return false;
	        }
        }

        /// <summary>
        /// Copies a given number of bytes from a file to another file.
        /// </summary>
        /// <param name="source">Source file to read from.</param>
        /// <param name="destination">Destination file to write to.</param>
        /// <param name="bytesToRead">Bytes to copy from <paramref name="source"/> to <paramref name="destination"/>.</param>
        public static void CopyFile(string source, string destination, int bytesToRead)
        {
	        var buffer = new byte[bytesToRead];

	        try
	        {
		        Directory.CreateDirectory(Path.GetDirectoryName(destination));
	        }
	        catch (ArgumentException)
	        {
		        // There's no directory in destination path, so nothing to create here.
	        }

	        using (var reader = new BinaryReader(new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
	        {
		        reader.Read(buffer, 0, bytesToRead);
	        }

	        File.WriteAllBytes(destination, buffer);
        }  
    }
}