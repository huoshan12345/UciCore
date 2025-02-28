namespace UciCore;

/// <summary>
/// Represents a UCI (Unified Configuration Interface) configuration,
/// encapsulating a collection of configuration sections associated with a 
/// specific package. The class supports rendering the configuration to a 
/// formatted string representation suitable for inclusion in UCI 
/// configuration files.
/// </summary>
[DebuggerDisplay("package {PackageName}")]
public class UciConfig
{
    /// <summary>
    /// Gets or sets the name of the package associated with this UCI configuration.
    /// This property defines the context for the configuration, allowing multiple 
    /// configurations to coexist for different packages.
    /// </summary>
    public string PackageName { get; set; } = "";

    /// <summary>
    /// Gets or sets the list of sections contained in this UCI configuration.
    /// Each section is represented by a <see cref="UciSection"/> object, which can 
    /// contain multiple options and comments.
    /// </summary>
    public List<UciSection> Sections { get; set; } = [];

    /// <summary>
    /// Gets or sets the full line comments associated with the entire configuration.
    /// These comments can provide additional context or documentation for the 
    /// configuration and may appear above the package declaration or sections.
    /// </summary>
    public List<string> FullLineComments { get; set; } = [];

    /// <summary>
    /// Returns a string representation of the UCI configuration.
    /// This method uses a helper to build the string by rendering the 
    /// configuration and its sections.
    /// </summary>
    /// <returns>A formatted string representation of the UCI configuration.</returns>
    public override string ToString()
    {
        return StringBuilderHelper.Build(Render);
    }

    /// <summary>
    /// Renders the UCI configuration to the provided <see cref="StringBuilder"/>. 
    /// The output includes the package name (if specified) followed by each 
    /// section rendered on a new line.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> to which the 
    /// rendered output will be appended.</param>
    public void Render(StringBuilder builder)
    {
        if (PackageName is { Length: > 0 })
        {
            builder.Append("package ");
            builder.AppendSingleQuoted(PackageName);
            builder.AppendLineFeed();
        }

        foreach (var (_, section, _, isLast) in Sections.IndexEx())
        {
            section.Render(builder);

            if (isLast == false)
                builder.AppendLineFeed();
        }
    }
}