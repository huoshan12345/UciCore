namespace System.Text;

internal static class StringBuilderExtensions
{
    public static StringBuilder AppendQuoted(this StringBuilder builder, char openingQuote, string value, char? closingQuote = null, char escapeCharacter = '\\')
    {
        closingQuote ??= openingQuote;
        builder.Append(openingQuote);

        foreach (var ch in value)
        {
            // Escape any quotes, closing quotes, or escape characters in the string
            if (ch == openingQuote || ch == closingQuote || ch == escapeCharacter)
            {
                builder.Append(escapeCharacter);
            }
            builder.Append(ch);  // Append the actual character
        }

        builder.Append(closingQuote);
        return builder;
    }

    public static StringBuilder AppendSingleQuoted(this StringBuilder builder, string value, char escapeCharacter = '\\')
    {
        return builder.AppendQuoted('\'', value, null, escapeCharacter);
    }

    public static StringBuilder AppendLineFeed(this StringBuilder builder)
    {
        return builder.Append('\n');
    }
}