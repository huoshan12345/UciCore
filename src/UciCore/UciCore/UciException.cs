namespace UciCore;

/// <summary>
/// Represents an exception that occurs during the processing of UCI (Unified 
/// Configuration Interface) configuration files. This class extends the base 
/// <see cref="Exception"/> class to include additional information about the 
/// position in the input text where the error occurred, making it easier to 
/// diagnose and handle errors related to UCI syntax.
/// </summary>
[Serializable]
public class UciException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UciException"/> class 
    /// with a specified error message, position, line, and column number.
    /// An optional inner exception can also be provided for exception 
    /// chaining.
    /// </summary>
    /// <param name="message">The error message that describes the error.</param>
    /// <param name="position">The number of characters read within the whole 
    /// input text before the exception (starting at 0).</param>
    /// <param name="line">The number of lines read so far before the exception 
    /// (starting at 0).</param>
    /// <param name="column">The number of characters read within the current 
    /// line before the exception (starting at 0).</param>
    /// <param name="innerException">The inner exception that is the cause of 
    /// this exception, or null if no inner exception is specified.</param>
    public UciException(string? message, int? position, int? line, int? column, Exception? innerException = null)
        : base(message, innerException)
    {
        Position = position;
        Line = line;
        Column = column;
    }

    /// <summary>
    /// Gets the number of characters read within the whole input text before the exception (starting at 0).
    /// </summary>
    public int? Position { get; }

    /// <summary>
    /// Gets the number of lines read so far before the exception (starting at 0).
    /// </summary>
    public int? Line { get; }

    /// <summary>
    /// Gets the number of characters read within the current line before the exception (starting at 0).
    /// </summary>
    public int? Column { get; }
}