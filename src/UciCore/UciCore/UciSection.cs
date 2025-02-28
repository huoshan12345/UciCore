namespace UciCore;

/// <summary>
/// Represents a section in a UCI (Unified Configuration Interface) configuration file.
/// Each section contains a type, a name, a collection of options, and associated comments.
/// </summary>
[DebuggerDisplay("config {Type} {Name}")]
public class UciSection
{
    /// <summary>
    /// Gets or sets the type of the UCI section.
    /// This indicates the category of the configuration, such as 'network', 'wireless', etc.
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// Gets or sets the name of the UCI section.
    /// This serves as the identifier for the specific configuration within the section.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the options associated with this UCI section.
    /// This is a collection of <see cref="UciOption"/> objects that represent key-value pairs 
    /// within the section.
    /// </summary>
    public List<UciOption> Options { get; set; } = [];

    /// <summary>
    /// Gets or sets the full line comments associated with this section.
    /// These comments can provide additional context or documentation for the section.
    /// </summary>
    public List<string> FullLineComments { get; set; } = [];

    /// <summary>
    /// Gets or sets the inline comment associated with this section.
    /// This comment appears directly after the section declaration on the same line.
    /// </summary>
    public string InlineComment { get; set; } = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="UciSection"/> class.
    /// Default constructor for creating an empty UCI section.
    /// </summary>
    public UciSection() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UciSection"/> class with a specified type, name,
    /// and an optional array of UCI options.
    /// </summary>
    /// <param name="type">The type of the UCI section.</param>
    /// <param name="name">The name of the UCI section.</param>
    /// <param name="options">An optional array of <see cref="UciOption"/> objects to include in the section.</param>
    public UciSection(string type, string name = "", params UciOption[] options)
    {
        Type = type;
        Name = name;
        Options.AddRange(options);
    }

    /// <summary>
    /// Returns a string representation of the UCI section.
    /// This method uses a helper to build the string by rendering the section and its options.
    /// </summary>
    /// <returns>A formatted string representation of the UCI section.</returns>
    public override string ToString()
    {
        return StringBuilderHelper.Build(Render);
    }

    /// <summary>
    /// Renders the UCI section to the provided <see cref="StringBuilder"/>. 
    /// The output includes the section declaration with its type and name, followed by 
    /// each option rendered on a new line.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> to which the rendered 
    /// string will be appended.</param>
    public void Render(StringBuilder builder)
    {
        builder.Append("config");
        builder.Append(' ');
        builder.Append(Type);
        if (Name is { Length: > 0 })
        {
            builder.Append(' ');
            builder.AppendSingleQuoted(Name);
        }
        builder.AppendLineFeed();

        foreach (var option in Options)
        {
            builder.Append('\t');
            option.Render(builder);
            builder.AppendLineFeed();
        }
    }

    public void Deconstruct(out string type, out string name, out List<UciOption> options)
    {
        type = Type;
        name = Name;
        options = Options;
    }
}
