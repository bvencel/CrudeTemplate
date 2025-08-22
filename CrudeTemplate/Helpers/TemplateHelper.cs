using CrudeTemplate.Constants;
using CrudeTemplate.Extensions;

namespace CrudeTemplate.Helpers;

/// <summary>
/// The templating solution uses the delimiters defined by <see cref="TemplateDelimiters.PlaceholderStart"/> and <see cref="TemplateDelimiters.PlaceholderEnd"/> around placeholders.
/// If the delimiter characters themselves need to be used in the template output, they can be escaped using the <see cref="TemplateDelimiters.GreekEscapeCharacter"/> constant:
/// - Use GreekEscapeCharacter + PlaceholderStart to output the character(s) represented by <see cref="TemplateDelimiters.PlaceholderStart"/>
/// - Use PlaceholderEnd + GreekEscapeCharacter to output the character(s) represented by <see cref="TemplateDelimiters.PlaceholderEnd"/>
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

        if (valueComponents is null || valueComponents.Count == 0)
        {
            return textWithPlaceholders;
        }

        foreach (KeyValuePair<string, string> component in valueComponents)
        {
            textWithPlaceholders = textWithPlaceholders.Replace(
                component.Key.AsPlaceholder(),
                string.IsNullOrEmpty(component.Value) ? string.Empty : component.Value);
        }

        return textWithPlaceholders;
    }

    /// <summary>
    /// Converts all GreekEscapeCharacter + PlaceholderStart and PlaceholderEnd + GreekEscapeCharacter to the literal text represented by <see cref="TemplateDelimiters.PlaceholderStart"/> and <see cref="TemplateDelimiters.PlaceholderEnd"/>, respectively.
    /// The placeholder delimiters (see <see cref="TemplateDelimiters.PlaceholderStart"/> and <see cref="TemplateDelimiters.PlaceholderEnd"/>) are reserved for placeholders. If the delimiter characters are needed in the output, they must be escaped as described using <see cref="TemplateDelimiters.GreekEscapeCharacter"/>.
    /// </summary>
    /// <param name="textToBeProcessed">The text that needs processing. Cannot be null.</param>
    /// <returns>A processed text where all GreekEscapeCharacter + PlaceholderStart and PlaceholderEnd + GreekEscapeCharacter are converted to the text represented by <see cref="TemplateDelimiters.PlaceholderStart"/> and <see cref="TemplateDelimiters.PlaceholderEnd"/> respectively.</returns>
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