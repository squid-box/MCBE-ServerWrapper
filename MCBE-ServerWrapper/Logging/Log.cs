namespace AhlSoft.BedrockServerWrapper.Logging
{
    using System;
    using System.IO;

    /// <inheritdoc cref="ILog" />
    public class Log : ILog
    {
        private const string LogFilePath = "mcbsw.log";
        private readonly ConsoleColor _originalConsoleColor;

        /// <summary>
        /// Creates a new <see cref="Log"/>.
        /// </summary>
        public Log()
        {
            _originalConsoleColor = Console.ForegroundColor;
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            WriteMessage("INFO", message, _originalConsoleColor);
        }

        /// <inheritdoc />
        public void Warning(string message)
        {
            WriteMessage("WARN", message, ConsoleColor.Yellow);
        }

        /// <inheritdoc />
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
            
            File.AppendAllText(LogFilePath, formattedMessage + Environment.NewLine);
        }
    }
}