namespace BedrockServerWrapper
{
    /// <summary>
    /// Possible exit codes.
    /// </summary>
    public static class ExitCodes
    {
        /// <summary>
        /// Everything went OK.
        /// </summary>
        public static int Ok => 0;

        /// <summary>
        /// The number of arguments were invalid.
        /// </summary>
        public static int InvalidNumberOfArguments => 1;

        /// <summary>
        /// The server files were invalid.
        /// </summary>
        public static int InvalidServerFiles => 2;

        /// <summary>
        /// Unhandled exception.
        /// </summary>
        public static int UnknownCrash => 3;
    }
}