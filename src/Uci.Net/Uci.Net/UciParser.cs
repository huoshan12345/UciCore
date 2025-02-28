using System.Linq;
using System.Security;

namespace Uci.Net;

/// <summary>
/// Provides static methods for parsing UCI (Unified Configuration Interface) configuration files.
/// Converts tokenized input into structured configuration objects representing the UCI hierarchy.
/// </summary>
public static class UciParser
{
    /// <summary>
    /// Parses UCI configuration text into a structured <see cref="UciConfig"/> object.
    /// The parser processes tokens from a lexer to build a complete representation
    /// of the configuration including package name, sections, and options.
    /// </summary>
    /// <param name="input">The UCI configuration text to parse.</param>
    /// <returns>
    /// A <see cref="UciConfig"/> object representing the parsed configuration.
    /// The object contains the package name and a collection of sections with their options.
    /// </returns>
    /// <exception cref="UciException">
    /// Thrown when a parsing error occurs, such as syntax errors or invalid token sequences.
    /// The exception includes position information for error reporting.
    /// </exception>
    public static UciConfig Parse(string input)
    {
        var lexer = new UciLexer(input);
        var config = new UciConfig();
        UciSection? section = null;

        using var e = lexer.LexTokens().GetEnumerator();

        while (e.MoveNext())
        {
            var it = e.Current;

            switch (it.Type)
            {
                case UciTokenType.PackageName:
                {
                    config.PackageName = it.Value;
                    break;
                }
                case UciTokenType.SectionType:
                {
                    section = new UciSection { Type = it.Value };
                    config.Sections.Add(section);
                    break;
                }
                case UciTokenType.Error:
                {
                    throw it.ToException("Encountered an error: " + it.Value);
                }
                case UciTokenType.Comment:
                {
                    //TODO: support comments.
                    break;
                }
                case UciTokenType.LineFeed:
                {
                    break;
                }
                case UciTokenType.OptionName:
                case UciTokenType.ListName:
                {
                    if (section is null)
                        throw it.ToException($"Encountered a option or list name '{it.Value}' without a preceding section.");

                    var isList = it.Type == UciTokenType.ListName;
                    var key = isList ? "list" : "option";

                    if (e.MoveNext())
                    {
                        var next = e.Current!;

                        if (next.Type != UciTokenType.OptionValue)
                            throw it.ToException($"Expected a value but got a {next.Value} after {key} '{it.Value}'.");

                        var option = new UciOption(it.Value, next.Value, isList);
                        section.Options.Add(option);
                    }
                    else
                    {
                        throw it.ToException($"Expected a value but got EOF after {key} '{it.Value}'.");
                    }

                    break;
                }
                case UciTokenType.Eof:
                {
                    if (e.MoveNext())
                        throw it.ToException("Unexpected EOF token.");

                    break;
                }
                case UciTokenType.OptionValue:
                {
                    throw it.ToException($"Encountered a value '{it.Value}' without a preceding option or list.");
                }
                case UciTokenType.SectionName:
                {
                    if (section is null || string.IsNullOrEmpty(section.Type))
                        throw it.ToException($"Encountered a section name '{it.Value}' without a preceding section type.");

                    // NOTE: uci does allow multiple sections with the same name but different types.
                    // uci: Parse error (section of different type overwrites prior section with same name)
                    // sections with the same name are merged into a single section (later options will override previous ones).

                    if (it.Value is not { Length: > 0 } name)
                        continue;

                    var exist = config.Sections.FirstOrDefault(m => m.Name == name && m != section); // NOTE: the section may not be the last one.
                    if (exist == null)
                    {
                        section.Name = name;
                        continue;
                    }

                    if (section.Type != exist.Type)
                        throw it.ToException($"Parse error (section of different type '{section.Type}' overwrites prior section '{exist.Type}' with same name '{name}').");

                    foreach (var option in section.Options)
                    {
                        exist.Options.Add(option);
                    }

                    config.Sections.Remove(section); // NOTE: the section may not be the last one.
                    section = exist;

                    break;
                }
                default:
                {
                    throw it.ToException($"Unexpected token type '{it.Type}'.");
                }
            }
        }

        // NOTE: how to handle duplicate options with the same key in the same section:
        // 1) exist' option is list, section' option is list => append
        // 2) exist' option is list, section' option is not list => overwrite
        // 3) exist' option is not list, section' option is list => append
        // 4) exist' option is not list, section' option is not list => overwrite
        foreach (var sec in config.Sections)
        {
            var options = sec.Options;
            var handled = new HashSet<int>();
            var remove = new SortedSet<int>();
            for (var i = options.Count - 1; i > 0; i--) // reverse order and exclude the first one
            {
                if (handled.Contains(i))
                    continue;

                var option = options[i];

                for (var j = 0; j < i - 1; j++)
                {
                    var op = options[j];
                    if (op.Key != option.Key)
                        continue;

                    if (option.IsList)
                    {
                        options[j].IsList = true;
                    }
                    else
                    {
                        remove.Add(j);
                    }

                    handled.Add(i);
                }
            }

            // loop in reverse order to remove items by index then you don't need worry about whether index is out of range.
            for (var i = remove.Count - 1; i >= 0; i--)
            {
                options.RemoveAt(i);
            }
        }

        return config;
    }
}

file static class Extensions
{
    public static UciException ToException(this UciToken token, string message)
    {
        return new UciException($"Invalid config at line {token.Line}: {message}", token.Position, token.Line, token.Column);
    }
}
