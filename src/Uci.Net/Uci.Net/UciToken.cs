namespace Uci.Net;

[DebuggerDisplay("{Type}, {Value}")]
public record UciToken(
    UciTokenType Type,
    string Value,
    int Position,
    int Line,
    int Column)
{
    public override string ToString()
    {
        return $"{nameof(UciToken)}({Type}, {Value})";
    }
}