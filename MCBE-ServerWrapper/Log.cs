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

        public void Info(string message)
        {
            WriteMessage("INFO", message, _originalConsoleColor);
        }

        public void Warning(string message)
        {
            WriteMessage("WARN", message, ConsoleColor.Yellow);
        }

        public void Error(string message)
        {
            WriteMessage(" ERR", message, ConsoleColor.DarkRed);
        }

        private void WriteMessage(string level, string message, ConsoleColor consoleColor)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var formattedMessage = $"|{level}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{message}";

            Console.ForegroundColor = consoleColor;
            Console.Out.WriteLine(formattedMessage);
            Console.ForegroundColor = _originalConsoleColor;
            
            File.AppendAllText(_settings.LogFilePath, formattedMessage + Environment.NewLine);
        }
    }
}