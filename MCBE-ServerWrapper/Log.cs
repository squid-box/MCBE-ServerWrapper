namespace BedrockServerWrapper
{
    using System;
    using System.IO;

    /// <summary>
    /// Logging class.
    /// </summary>
    public class Log
    {
        private readonly Settings _settings;
        private readonly ConsoleColor _originalConsoleColor;

        public Log(Settings settings)
        {
            _settings = settings;
            _originalConsoleColor = Console.ForegroundColor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            WriteMessage("INFO", message, _originalConsoleColor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Warning(string message)
        {
            WriteMessage("WARNING", message, ConsoleColor.Yellow);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message)
        {
            WriteMessage("ERROR", message, ConsoleColor.DarkRed);
        }

        private void WriteMessage(string level, string message, ConsoleColor consoleColor)
        {
            var formattedMessage = $"|{level}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{message}";

            Console.ForegroundColor = consoleColor;
            Console.Out.WriteLine(formattedMessage);
            Console.ForegroundColor = _originalConsoleColor;
            
            File.AppendAllText(_settings.LogFilePath, formattedMessage + Environment.NewLine);
        }
    }
}