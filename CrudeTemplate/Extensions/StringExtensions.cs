using CrudeTemplate.Constants;

namespace CrudeTemplate.Extensions;

public static class StringExtensions
{
    public static string AsPlaceholder(this string key)
    {
        return $"{TemplateDelimiters.PlaceholderStart}{key}{TemplateDelimiters.PlaceholderEnd}";
    }
}
