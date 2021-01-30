namespace BedrockServerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
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

        public static string TimePlayedConversion(int minutes)
        {
            var timespan = TimeSpan.FromMinutes(minutes);

            return $"{Math.Floor(timespan.TotalHours):00}h {timespan.Minutes:00}m";
        }

        /// <summary>
        /// Gets the version of this program.
        /// </summary>
        public static string ProgramVersion => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

        public static bool DeleteDirectory(string dir)
        {
	        Console.Out.WriteLine("Removing temporary files.");
	        
	        try
	        {
		        Directory.Delete(dir, true);
		        return true;
	        }
	        catch (Exception e)
	        {
		        Console.WriteLine($"Couldn't delete temporary files/folder: {e.GetType()} - {e.Message}");
		        return false;
	        }
        }

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

	        using (var reader = new BinaryReader(new FileStream(source, FileMode.Open)))
	        {
		        reader.Read(buffer, 0, bytesToRead);
	        }

	        File.WriteAllBytes(destination, buffer);
        }  
    }
}