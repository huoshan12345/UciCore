namespace Uci.Net;

public static class Extensions
{
    public static UciToken Make(this UciTokenType type, string val)
    {
        return new(type, val, 0, 0, 0);
    }

    public static TResult Pipe<T, TResult>(this T value, Func<T, TResult> func) => func(value);
}

public static class UciTokens
{
    public static UciToken PackageName(string name)
    {
        return UciTokenType.PackageName.Make(name);
    }

    public static UciToken[] Option(string key, string value)
    {
        return [UciTokenType.OptionName.Make(key), UciTokenType.OptionValue.Make(value)];
    }

    public static UciToken[] List(string key, string value)
    {
        return [UciTokenType.ListName.Make(key), UciTokenType.OptionValue.Make(value)];
    }

    public static UciToken Comment(string value)
    {
        return UciTokenType.Comment.Make(value);
    }

    public static UciToken[] Section(string type, string name = "")
    {
        return [UciTokenType.SectionType.Make(type), UciTokenType.SectionName.Make(name)];
    }
}