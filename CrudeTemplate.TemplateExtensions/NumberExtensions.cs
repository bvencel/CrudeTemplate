using System.Globalization;

namespace CrudeTemplate.TemplateExtensions;

/// <summary>
/// Extension methods for formatting numeric and currency values in templates.
/// This is just a proof of concept.
/// </summary>
public static class NumberExtensions
{
    /// <summary>
    /// Formats a decimal as a culture-specific currency string and adds it as a child template.
    /// </summary>
    /// <param name="template">The parent template. Cannot be null.</param>
    /// <param name="placeholder">The name of the placeholder.</param>
    /// <param name="amount">The numeric value to format.</param>
    /// <param name="cultureInfo">The culture info to use for formatting. Cannot be null.</param>
    /// <returns>This <see cref="Template"/> instance with the currency text added.</returns>
    public static Template WithCurrency(
        this Template template,
        string placeholder,
        decimal amount,
        CultureInfo cultureInfo)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        string formattedCurrency = amount.ToString("C", cultureInfo);
        return template.WithText(placeholder, formattedCurrency);
    }

    /// <summary>
    /// Formats a decimal as a culture-specific number string using the general numeric ("N") format
    /// and adds it as a child template.
    /// </summary>
    /// <param name="template">The parent template. Cannot be null.</param>
    /// <param name="placeholder">The name of the placeholder.</param>
    /// <param name="value">The numeric value to format.</param>
    /// <param name="cultureInfo">The culture info to use for formatting. Cannot be null.</param>
    /// <returns>This <see cref="Template"/> instance with the formatted number text added.</returns>
    public static Template WithNumber(
        this Template template,
        string placeholder,
        decimal value,
        CultureInfo cultureInfo)
    {
        return template.WithNumber(placeholder, value, "N", cultureInfo);
    }

    /// <summary>
    /// Formats a decimal as a culture-specific number string and adds it as a child template.
    /// </summary>
    /// <param name="template">The parent template. Cannot be null.</param>
    /// <param name="placeholder">The name of the placeholder.</param>
    /// <param name="value">The numeric value to format.</param>
    /// <param name="format">
    /// A standard or custom numeric format string (e.g. "N2", "F0", "0.##").
    /// </param>
    /// <param name="cultureInfo">The culture info to use for formatting. Cannot be null.</param>
    /// <returns>This <see cref="Template"/> instance with the formatted number text added.</returns>
    public static Template WithNumber(
        this Template template,
        string placeholder,
        decimal value,
        string format,
        CultureInfo cultureInfo)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        string formattedNumber = value.ToString(format, cultureInfo);

        return template.WithText(placeholder, formattedNumber);
    }
}