using System;
using System.Text;

namespace OpenApiSourceGenerator;

public static class StringExtensions
{
    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var span = input.AsSpan();
        var sb = new StringBuilder(span.Length);

        var capitalize = true;

        foreach (var ch in span)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(capitalize ? char.ToUpperInvariant(ch) : ch);
                capitalize = false;
            }
            else
            {
                capitalize = true;
            }
        }

        return sb.ToString();
    }
}