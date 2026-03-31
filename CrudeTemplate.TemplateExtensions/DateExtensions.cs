using System.Globalization;

namespace CrudeTemplate.TemplateExtensions;

/// <summary>
/// Extension methods for adding formatted and localized dates to templates.
/// </summary>
public static class DateExtensions
{
    /// <summary>
    /// Formats a date using the short date pattern (<c>"d"</c>) for the provided culture and adds it as a child template text.
    /// </summary>
    /// <example>
    /// <code language="csharp">
    /// Template template = new Template("Date: {{Date}}")
    ///     .WithLocalizedDate("Date", new DateTime(2025, 3, 15), CultureInfo.GetCultureInfo("de-DE"));
    /// // Renders: "Date: 15.03.2025"
    /// </code>
    /// </example>
    /// <param name="template">The parent template. Cannot be <see langword="null"/>.</param>
    /// <param name="placeholder">The name of the placeholder. Must not be <see langword="null"/>, empty or whitespace.</param>
    /// <param name="date">The date to format.</param>
    /// <param name="cultureInfo">The culture info to use for formatting. Cannot be <see langword="null"/>.</param>
    /// <returns>The parent <see cref="Template"/> instance with the specified child text added or replaced.</returns>
    public static Template WithLocalizedDate(
        this Template template,
        string placeholder,
        DateTime date,
        CultureInfo cultureInfo)
    {
        return template.WithLocalizedDate(placeholder, date, "d", cultureInfo);
    }

    /// <summary>
    /// Formats a date using the provided format string and culture and adds it as a child template text.
    /// </summary>
    /// <example>
    /// <code language="csharp">
    /// Template template = new Template("Date: {{Date}}")
    ///     .WithLocalizedDate("Date", new DateTime(2025, 12, 31), "yyyy-MM-dd", CultureInfo.InvariantCulture);
    /// // Renders: "Date: 2025-12-31"
    /// </code>
    /// </example>
    /// <param name="template">The parent template. Cannot be <see langword="null"/>.</param>
    /// <param name="placeholder">The name of the placeholder. Must not be <see langword="null"/>, empty or whitespace.</param>
    /// <param name="date">The date to format.</param>
    /// <param name="format">A standard or custom date format string (e.g. <c>"d"</c>, <c>"yyyy-MM-dd"</c>, <c>"D"</c>).</param>
    /// <param name="cultureInfo">The culture info to use for formatting. Cannot be <see langword="null"/>.</param>
    /// <returns>The parent <see cref="Template"/> instance with the specified child text added or replaced.</returns>
    public static Template WithLocalizedDate(
        this Template template,
        string placeholder,
        DateTime date,
        string format,
        CultureInfo cultureInfo)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        string formattedDate = date.ToString(format, cultureInfo);

        return template.WithText(placeholder, formattedDate);
    }
}