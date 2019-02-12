namespace BedrockServerWrapper
{
    /// <summary>
    /// 
    /// </summary>
    public static class ExitCodes
    {
        /// <summary>
        /// 
        /// </summary>
        public static int Ok => 0;

        /// <summary>
        /// 
        /// </summary>
        public static int InvalidNumberOfArguments => 1;

        /// <summary>
        /// 
        /// </summary>
        public static int InvalidServerFiles => 2;

        /// <summary>
        /// Unhandled exception.
        /// </summary>
        public static int UnknownCrash => 3;
    }
}
