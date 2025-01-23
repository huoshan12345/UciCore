namespace Uci.Net;

public static class UciParser
{
    public static UciConfig Parse(string input)
    {
        var lexer = new UciLexer(input);
        var config = new UciConfig();
        UciSection? section = null;

        using var e = lexer.LexTokens().GetEnumerator();

        while (e.MoveNext())
        {
            var it = e.Current!;

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

                    section.Name = it.Value;
                    break;
                }
                default:
                {
                    throw it.ToException($"Unexpected token type '{it.Type}'.");
                }
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
