namespace AhlSoft.BedrockServerWrapper;

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
    /// The server files were invalid.
    /// </summary>
    public static int InvalidServerFiles => 2;

    /// <summary>
    /// Unhandled exception.
    /// </summary>
    public static int UnknownCrash => 3;
}