namespace AhlSoft.BedrockServerWrapper.Logging
{
    /// <summary>
    /// Logging class.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Log a message at the Info level.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Info(string message);

        /// <summary>
        /// Log a message at the Warning level.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Warning(string message);

        /// <summary>
        /// Log a message at the Error level.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Error(string message);
    }
}