namespace AhlSoft.BedrockServerWrapper.Logging
{
    using System;
    using System.IO;

    using Spectre.Console;

    /// <inheritdoc cref="ILog" />
    public class Log : ILog
    {
        private static readonly object LogLock = new();

        private const string LogFilePath = "mcbsw.log";

        /// <inheritdoc />
        public void Info(string message, string color = "green", bool logToConsole = true)
        {
            WriteMessage("INFO", message, logToConsole, color);
        }

        /// <inheritdoc />
        public void Warning(string message, string color = "yellow", bool logToConsole = true)
        {
            WriteMessage("WARN", message, logToConsole, color);
        }

        /// <inheritdoc />
        public void Error(string message, string color = "red", bool logToConsole = true)
        {
            WriteMessage(" ERR", message, logToConsole, color);
        }

        /// <inheritdoc />
        public void Exception(Exception exception, string color = "maroon", bool logToConsole = true)
        {
            var count = 0;

            while (exception != null)
            {
                WriteMessage("  EX", $"{(count != 0 ? $" {count}. " : string.Empty)}{exception.GetType().Name}: {exception.Message}", logToConsole, color);
                exception = exception.InnerException;
                count++;
            }
        }

        private static void WriteMessage(string level, string message, bool logToConsole, string format = "")
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            lock (LogLock)
            {
                var formattedMessage = $"|{level}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}| {message}";

                File.AppendAllText(LogFilePath, formattedMessage + Environment.NewLine);

                if (!logToConsole)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(format))
                {
                    var fancyMessage =
                        $"[grey]|[/][{format}]{level}[/][grey]|[/]{DateTime.Now:yyyy-MM-dd HH:mm:ss}[grey]|[/] [{format}]{message.EscapeMarkup()}[/]";
                    AnsiConsole.MarkupLine(fancyMessage);
                }
                else
                {
                    Console.WriteLine(formattedMessage);
                }
            }
        }
    }
}