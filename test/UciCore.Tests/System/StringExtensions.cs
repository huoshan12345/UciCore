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
}