namespace CrudeTemplate.TemplateExtensions;

/// <summary>
/// Extension methods for conditional template population.
/// </summary>
public static class ConditionExtensions
{
    /// <summary>
    /// Evaluates a condition and injects the corresponding text into the placeholder.
    /// </summary>
    /// <param name="template">The parent template. Cannot be null.</param>
    /// <param name="placeholder">The name of the placeholder. Must not be null, empty or whitespace.</param>
    /// <param name="condition">The boolean condition to evaluate.</param>
    /// <param name="trueText">The text to inject if the condition is true.</param>
    /// <param name="falseText">The text to inject if the condition is false. Defaults to an empty string.</param>
    /// <returns>This <see cref="Template"/> instance with the specified child text added or replaced.</returns>
    public static Template WithCondition(
        this Template template,
        string placeholder,
        bool condition,
        string trueText,
        string? falseText = null)
    {
        ArgumentNullException.ThrowIfNull(template);

        string resultingText = condition ? trueText : (falseText ?? string.Empty);
        return template.WithText(placeholder, resultingText);
    }
}