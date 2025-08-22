namespace CrudeTemplate.Constants;

/// <summary>
/// Centralized constants for template delimiters and escape characters.
/// </summary>
public class TemplateDelimiters
{
    public const string GreekEscapeCharacter = "Ø";
    public const string PlaceholderEnd = "}}";
    public const string PlaceholderStart = "{{";

    public static string EscapedEnd => $"{GreekEscapeCharacter}{PlaceholderEnd}";
    public static string EscapedStart => $"{GreekEscapeCharacter}{PlaceholderStart}";
}