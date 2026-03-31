using CrudeTemplate.Constants;

namespace CrudeTemplate.Extensions;

/// <summary>
/// Extension methods for <see cref="string"/> related to template placeholder formatting.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Wraps the given key in placeholder delimiters to produce a placeholder token
    /// (e.g. <c>"Name"</c> becomes <c>"{{Name}}"</c>).
    /// </summary>
    /// <param name="key">The placeholder name to wrap.</param>
    /// <returns>The key wrapped in <see cref="TemplateDelimiters.PlaceholderStart"/> and <see cref="TemplateDelimiters.PlaceholderEnd"/>.</returns>
    public static string AsPlaceholder(this string key)
    {
        return $"{TemplateDelimiters.PlaceholderStart}{key}{TemplateDelimiters.PlaceholderEnd}";
    }
}
