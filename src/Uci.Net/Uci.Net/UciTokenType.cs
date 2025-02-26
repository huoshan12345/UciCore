namespace Uci.Net;

/// <summary>
/// Defines the different types of tokens that can be recognized by the UCI (Unified 
/// Configuration Interface) lexer. Each token type represents a distinct 
/// component of the UCI configuration syntax, allowing for efficient parsing 
/// and interpretation of configuration files.
/// </summary>
public enum UciTokenType
{
    /// <summary>
    /// Indicates an error encountered during tokenization. This token type 
    /// is used to signal invalid input or unexpected syntax.
    /// </summary>
    Error,

    /// <summary>
    /// Represents the end of the file (EOF) token, which signifies that 
    /// there are no more tokens to read from the input stream.
    /// </summary>
    Eof,

    /// <summary>
    /// Represents the name of an option within a UCI section. This token type 
    /// is used to identify configuration settings.
    /// </summary>
    OptionName,

    /// <summary>
    /// Represents the value assigned to an option within a UCI section. This 
    /// token type holds the actual configuration value corresponding to an 
    /// option name.
    /// </summary>
    OptionValue,

    /// <summary>
    /// Represents the name of a list within a UCI configuration. This token 
    /// type is used when multiple values are associated with a single key.
    /// </summary>
    ListName,

    /// <summary>
    /// Represents a comment token, which is used to capture any comments in 
    /// the configuration file that are not part of the actual settings.
    /// </summary>
    Comment,

    /// <summary>
    /// Represents the type of a section within the UCI configuration. This 
    /// token type identifies the category of the section (e.g., "network", 
    /// "wireless").
    /// </summary>
    SectionType,

    /// <summary>
    /// Represents the name of a section within the UCI configuration. This 
    /// token type serves as an identifier for the specific section.
    /// </summary>
    SectionName,

    /// <summary>
    /// Represents the name of the package associated with the UCI configuration. 
    /// This token type defines the context for the configuration.
    /// </summary>
    PackageName,

    /// <summary>
    /// Represents a line feed token, indicating the end of a line in the 
    /// configuration file. This token type is used for managing line-based 
    /// structure in the configuration.
    /// </summary>
    LineFeed,
}