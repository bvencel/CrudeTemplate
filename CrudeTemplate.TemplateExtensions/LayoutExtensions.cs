namespace CrudeTemplate.TemplateExtensions;

/// <summary>
/// Extension methods for manipulating string layout in templates.
/// This is just a proof of concept.
/// </summary>
public static class LayoutExtensions
{
    /// <summary>
    /// Right-pads a string value to a specific total width before injecting it into the template.
    /// </summary>
    /// <param name="template">The parent template. Cannot be null.</param>
    /// <param name="placeholder">The name of the placeholder.</param>
    /// <param name="value">The string to pad. Cannot be null.</param>
    /// <param name="totalWidth">The total width of the resulting string.</param>
    /// <param name="paddingChar">The character used for padding. Defaults to a space.</param>
    /// <returns>This <see cref="Template"/> instance with the padded text added.</returns>
    public static Template WithPaddedRight(
        this Template template,
        string placeholder,
        string value,
        int totalWidth,
        char paddingChar = ' ')
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(value);

        string paddedText = value.PadRight(totalWidth, paddingChar);
        return template.WithText(placeholder, paddedText);
    }
}