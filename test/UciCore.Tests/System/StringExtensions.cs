using System.Diagnostics.CodeAnalysis;

namespace System;

public static class StringExtensions
{
    [return: NotNullIfNotNull(nameof(source))]
    public static string? TrimEnd(this string? source, string? trimString)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(trimString))
            return source;

        var result = source;
        while (result.EndsWith(trimString))
        {
            result = result[..^trimString.Length];
        }
        return result;
    }

    public static string LfToCrLf(this string input)
    {
        return input.Replace("\n", "\r\n");
    }

    public static string CrLfToLf(this string input)
    {
        return input.Replace("\r\n", "\n");
    }
}