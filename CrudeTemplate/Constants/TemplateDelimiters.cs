namespace CrudeTemplate.Constants;

/// <summary>
/// Centralized constants for template delimiters and escape characters used in the templating system.
/// <para>
/// <b>Placeholders:</b> Named tokens in templates are delimited by <see cref="PlaceholderStart"/> (<c>{{</c>) and <see cref="PlaceholderEnd"/> (<c>}}</c>).
/// For example, <c>{{Name}}</c> is a placeholder that can be replaced with a value.
/// </para>
/// <para>
/// <b>Escaping:</b> If the delimiter characters themselves need to appear in the output, wrap them with <see cref="EscapeCharacter"/> (<c>Ø</c>):
/// <list type="bullet">
/// <item><description>Use <see cref="EscapedStart"/> (<c>Ø{{</c>) to output a literal <see cref="PlaceholderStart"/></description></item>
/// <item><description>Use <see cref="EscapedEnd"/> (<c>}}Ø</c>) to output a literal <see cref="PlaceholderEnd"/></description></item>
/// </list>
/// </para>
/// <para>
/// <b>Example:</b> To render the text <c>{{literal}}</c>, use <c>Ø{{literal}}Ø</c> in the template.
/// </para>
/// </summary>
public static class TemplateDelimiters
{
    /// <summary>
    /// The escape character used to escape placeholder delimiters. Set to <c>Ø</c> (Scandinavian letter O-with-stroke).
    /// Prefix <see cref="PlaceholderStart"/> or suffix <see cref="PlaceholderEnd"/> with this character to output literal delimiters.
    /// </summary>
    public const string EscapeCharacter = "Ø";

    /// <summary>
    /// The closing delimiter for placeholders. Set to <c>}}</c>.
    /// </summary>
    public const string PlaceholderEnd = "}}";

    /// <summary>
    /// The opening delimiter for placeholders. Set to <c>{{</c>.
    /// </summary>
    public const string PlaceholderStart = "{{";

    /// <summary>
    /// The escape sequence for <see cref="PlaceholderEnd"/>. Renders as a literal <c>}}</c> in the final output.
    /// </summary>
    public const string EscapedEnd = PlaceholderEnd + EscapeCharacter;

    /// <summary>
    /// The escape sequence for <see cref="PlaceholderStart"/>. Renders as a literal <c>{{</c> in the final output.
    /// </summary>
    public const string EscapedStart = EscapeCharacter + PlaceholderStart;
}