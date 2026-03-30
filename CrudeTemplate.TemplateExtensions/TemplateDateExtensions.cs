using System.Globalization;

namespace CrudeTemplate.Extensions;

/// <summary>
/// Extension methods for adding formatted and localized dates to templates.
/// </summary>
public static class TemplateDateExtensions
{
    /// <summary>
    /// Formats a date using the provided culture and adds it as a child template text.
    /// </summary>
    /// <param name="template">The parent template. Cannot be null.</param>
    /// <param name="placeholder">The name of the placeholder. Must not be null, empty, or whitespace.</param>
    /// <param name="date">The date to format.</param>
    /// <param name="cultureInfo">The culture info to use for formatting. Cannot be null.</param>
    /// <param name="format">The standard or custom date format string. Defaults to short date pattern ("d").</param>
    /// <returns>The parent <see cref="Template"/> instance with the specified child text added or replaced.</returns>
    public static Template WithLocalizedDate(
        this Template template,
        string placeholder,
        DateTime date,
        CultureInfo cultureInfo,
        string format = "d")
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        // Format the date into a plain string *before* it enters the template tree
        string formattedDate = date.ToString(format, cultureInfo);

        return template.WithText(placeholder, formattedDate);
    }
}