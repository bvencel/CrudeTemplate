using CrudeTemplate.Constants;
using CrudeTemplate.Extensions;

namespace CrudeTemplate.Helpers;

/// <summary>
/// Helper methods for template rendering and processing.
/// For delimiter and escaping documentation, see <see cref="TemplateDelimiters"/>.
/// </summary>
public static class TemplateHelper
{
    /// <summary>
    /// Replace the placeholders with values in the given text.
    /// </summary>
    /// <param name="textWithPlaceholders">The text with placeholders. Cannot be null.</param>
    /// <param name="valueComponents">The components to be replaced. Cannot be null.</param>
    /// <returns>
    /// The text with replaced placeholders.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="textWithPlaceholders"/> or <paramref name="valueComponents"/> is null.</exception>
    public static string InjectPlaceholderValues(string textWithPlaceholders, Dictionary<string, string> valueComponents)
    {
        ArgumentNullException.ThrowIfNull(textWithPlaceholders);
        ArgumentNullException.ThrowIfNull(valueComponents);

        if (valueComponents.Count == 0)
        {
            return textWithPlaceholders;
        }

        foreach (KeyValuePair<string, string> component in valueComponents)
        {
            textWithPlaceholders = textWithPlaceholders.Replace(
                component.Key.AsPlaceholder(),
                component.Value);
        }

        return textWithPlaceholders;
    }

    /// <summary>
    /// Converts escaped placeholders to literal delimiter characters.
    /// See <see cref="TemplateDelimiters"/> for escape sequence details.
    /// </summary>
    /// <param name="textToBeProcessed">The text that needs processing. Cannot be null.</param>
    /// <returns>A processed text where escaped delimiters are converted to literal delimiters.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="textToBeProcessed"/> is null.</exception>
    public static string ReplaceEscapedPlaceholdersIfNeeded(string textToBeProcessed)
    {
        ArgumentNullException.ThrowIfNull(textToBeProcessed);

        if (string.IsNullOrEmpty(textToBeProcessed))
        {
            return textToBeProcessed;
        }

        textToBeProcessed = textToBeProcessed.Replace(TemplateDelimiters.EscapedStart, TemplateDelimiters.PlaceholderStart);
        textToBeProcessed = textToBeProcessed.Replace(TemplateDelimiters.EscapedEnd, TemplateDelimiters.PlaceholderEnd);

        return textToBeProcessed;
    }
}