namespace UciCore;

/// <summary>
/// Represents an option in a UCI (Unified Configuration Interface) configuration file.
/// Each option consists of a key-value pair, and it may represent a single option 
/// or a list of values. The class supports rendering the option to a formatted 
/// string representation suitable for inclusion in UCI configuration files.
/// </summary>
[DebuggerDisplay("{IsList ? \"list\" : \"option\"} {Key} {Value}")]
public class UciOption
{
    /// <summary>
    /// Gets or sets the key for this UCI option.
    /// This identifier is used to specify the configuration setting in the UCI system.
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// Gets or sets the value associated with this UCI option.
    /// This represents the setting or data corresponding to the specified key.
    /// For list options, this could be one of multiple values.
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// Gets or sets a value indicating whether this UCI option is a list.
    /// If true, the option represents a list of values rather than a single key-value pair.
    /// </summary>
    public bool IsList { get; set; } = false;

    /// <summary>
    /// Gets the full line comments associated with this option.
    /// These comments provide additional context or documentation for the option
    /// and appear on lines preceding or following the key-value pair.
    /// </summary>
    public List<string> FullLineComments { get; } = [];

    /// <summary>
    /// Gets or sets the inline comment associated with this option.
    /// This comment appears directly after the key-value pair on the same line.
    /// </summary>
    public string InlineComment { get; set; } = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="UciOption"/> class.
    /// Default constructor for creating an empty UCI option.
    /// </summary>
    public UciOption() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UciOption"/> class with a specified key, value,
    /// and an optional parameter indicating if it represents a list.
    /// </summary>
    /// <param name="key">The key for this UCI option.</param>
    /// <param name="value">The value for this UCI option.</param>
    /// <param name="isList">Indicates whether this option is a list.</param>
    public UciOption(string key, string value, bool isList = false)
    {
        Key = key;
        Value = value;
        IsList = isList;
    }

    /// <summary>
    /// Returns a string representation of the UCI option.
    /// This method uses a helper to build the string by rendering the option.
    /// </summary>
    /// <returns>A formatted string representation of the UCI option.</returns>
    public override string ToString()
    {
        return StringBuilderHelper.Build(Render);
    }

    /// <summary>
    /// Renders the UCI option to the provided <see cref="StringBuilder"/>. 
    /// The output format depends on whether the option represents a single value 
    /// or a list. It includes the type (either "option" or "list"), followed by 
    /// the key and the value, which is enclosed in single quotes.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> to which the 
    /// rendered output will be appended.</param>
    public void Render(StringBuilder builder)
    {
        builder.Append(IsList ? "list" : "option");
        builder.Append(' ');
        builder.Append(Key);
        builder.Append(' ');
        builder.AppendSingleQuoted(Value);
    }

    /// <summary>
    /// Deconstructs this UCI option into its key and value components.
    /// This method is useful for tuple deconstruction in C#.
    /// </summary>
    /// <param name="key">The key of this UCI option.</param>
    /// <param name="value">The value of this UCI option.</param>
    /// <param name="isList">The value indicating whether this UCI option is a list.</param>
    public void Deconstruct(out string key, out string value, out bool isList)
    {
        key = Key;
        value = Value;
        isList = IsList;
    }
}