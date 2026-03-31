using CrudeTemplate.Constants;
using CrudeTemplate.Extensions;

namespace CrudeTemplate;

/// <summary>
/// Represents a recursive and composable string template that supports named placeholders and child templates and render-time primitives.
/// <para>
/// Placeholders are named tokens in the template text and delimited by <c>{{</c> and <c>}}</c> (e.g. <c>{{Name}}</c>).
/// Placeholders can be replaced by structural child templates (assembled once) or primitives (injected at render time).
/// </para>
/// <para>
/// Mutability and cloning: The <see cref="Template"/> class is mutable. You can modify the <see cref="Text"/> property and the <see cref="ChildTemplates"/> dictionary.
/// It implements <see cref="ICloneable"/> to provide a deep copy of the template tree and allowing you to branch structural variations safely.
/// </para>
/// <para>
/// Placeholder validation: Placeholders must not be null or empty or whitespace. An <see cref="ArgumentException"/> is thrown if a placeholder is null or empty or consists only of whitespace.
/// </para>
/// </summary>
public class Template(string text, Dictionary<string, Template>? childTemplates = null) : ICloneable
{
    /// <summary>
    /// The maximum recursion depth allowed when rendering templates.
    /// Exceeding this limit indicates a likely circular reference in the template tree.
    /// </summary>
    private const int MaxRecursionDepth = 100;

    /// <summary>
    /// Shared empty dictionary used by the parameterless <see cref="Render()"/> overload to avoid allocating a new instance on every call.
    /// Safe to share because <see cref="RenderRecursively"/> never mutates the primitives dictionary.
    /// </summary>
    private static readonly Dictionary<string, string> EmptyPrimitives = [];

    /// <summary>
    /// Gets or sets the dictionary of child templates (structural placeholders). Never null.
    /// Each key corresponds to a placeholder name in the template text and the value is the <see cref="Template"/> to substitute.
    /// </summary>
    public Dictionary<string, Template> ChildTemplates
    {
        get => field;
        set => field = value ?? [];
    } = childTemplates ?? [];

    /// <summary>
    /// Gets or sets the template text. May contain placeholders (e.g. <c>{{Name}}</c>).
    /// </summary>
    public string Text { get; set; } = text ?? throw new ArgumentNullException(nameof(text));

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

        textToBeProcessed = textToBeProcessed.Replace(TemplateDelimiters.EscapedStart, TemplateDelimiters.PlaceholderStart);
        textToBeProcessed = textToBeProcessed.Replace(TemplateDelimiters.EscapedEnd, TemplateDelimiters.PlaceholderEnd);

        return textToBeProcessed;
    }

    /// <summary>
    /// Creates a deep copy of this template and recursively cloning all child templates.
    /// </summary>
    /// <returns>A new and deeply cloned <see cref="Template"/> instance.</returns>
    public object Clone()
    {
        Template clonedTemplate = CloneTemplate();

        return clonedTemplate;
    }

    /// <summary>
    /// Creates a deep copy of this template and recursively cloning all child templates.
    /// Strongly typed for convenience.
    /// </summary>
    /// <returns>A new and deeply cloned <see cref="Template"/> instance.</returns>
    public Template CloneTemplate()
    {
        Dictionary<string, Template> clonedChildren = [];

        foreach (KeyValuePair<string, Template> childTemplatePair in ChildTemplates)
        {
            clonedChildren.Add(childTemplatePair.Key, childTemplatePair.Value.CloneTemplate());
        }

        Template newTemplate = new(Text, clonedChildren);

        return newTemplate;
    }

    /// <summary>
    /// Renders this template and all its child templates recursively and replacing all structural placeholders.
    /// This method is purely functional and does not mutate the template instance.
    /// </summary>
    /// <returns>The fully rendered template string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the template tree exceeds the maximum recursion depth and indicating a likely circular reference.</exception>
    public string Render()
    {
        string processed = RenderRecursively(this, EmptyPrimitives, 0);
        processed = ReplaceEscapedPlaceholdersIfNeeded(processed);

        return processed;
    }

    /// <summary>
    /// Renders this template and all its child templates recursively and replacing all placeholders with structure and runtime data.
    /// This method is purely functional and does not mutate the template instance.
    /// </summary>
    /// <param name="primitives">A dictionary of primitive string values to inject at render time (e.g. exact timestamps and names). Cannot be null.</param>
    /// <returns>The fully rendered template string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="primitives"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the template tree exceeds the maximum recursion depth and indicating a likely circular reference.</exception>
    public string Render(IDictionary<string, string> primitives)
    {
        ArgumentNullException.ThrowIfNull(primitives);

        string processed = RenderRecursively(this, primitives, 0);
        processed = ReplaceEscapedPlaceholdersIfNeeded(processed);

        return processed;
    }

    /// <summary>
    /// Adds or replaces a child template for the given placeholder key.
    /// </summary>
    /// <param name="placeholder">The name of the placeholder to associate with the child template. Must not be null or empty or whitespace.</param>
    /// <param name="childTemplate">The <see cref="Template"/> to insert or replace. May be null (treated as an empty template).</param>
    /// <returns>This <see cref="Template"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="placeholder"/> is null or empty or whitespace.</exception>
    public Template With(string placeholder, Template? childTemplate)
    {
        if (string.IsNullOrWhiteSpace(placeholder))
        {
            throw new ArgumentException("Placeholder must not be null or empty or whitespace.", nameof(placeholder));
        }

        Template finalChildTemplate = childTemplate is null ? new Template(string.Empty) : childTemplate;
        ChildTemplates[placeholder] = finalChildTemplate;

        return this;
    }

    /// <summary>
    /// Adds or replaces a child template for the given placeholder key and using a string as the child template text.
    /// </summary>
    /// <param name="placeholder">The name of the placeholder to associate with the child template. Must not be null or empty or whitespace.</param>
    /// <param name="childText">The text for the new child template. May be null (treated as empty string).</param>
    /// <returns>This <see cref="Template"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="placeholder"/> is null or empty or whitespace.</exception>
    public Template WithText(string placeholder, string? childText)
    {
        string finalText = childText is null ? string.Empty : childText;
        Template newChildTemplate = new(finalText);

        return With(placeholder, newChildTemplate);
    }

    /// <summary>
    /// Recursively renders the template and all its child templates.
    /// </summary>
    /// <param name="templateToProcess">The template to process in the current recursion step.</param>
    /// <param name="primitives">A dictionary of primitive string values to inject at render time.</param>
    /// <param name="depth">The current depth of recursion to prevent infinite loops.</param>
    /// <returns>The fully rendered string for the current template context.</returns>
    /// <exception cref="InvalidOperationException">Thrown when recursion depth exceeds <see cref="MaxRecursionDepth"/>.</exception>
    private static string RenderRecursively(Template templateToProcess, IDictionary<string, string> primitives, int depth)
    {
        if (depth > MaxRecursionDepth)
        {
            throw new InvalidOperationException($"Template recursion exceeded maximum depth of {MaxRecursionDepth}. This may indicate a circular reference in the template tree.");
        }

        Dictionary<string, string> finalTemplateValuesForPlaceholders = new(primitives);

        foreach (KeyValuePair<string, Template> childTemplatePair in templateToProcess.ChildTemplates)
        {
            string processedComponentText = RenderRecursively(childTemplatePair.Value, primitives, depth + 1);
            finalTemplateValuesForPlaceholders[childTemplatePair.Key] = processedComponentText;
        }

        string injectedText = InjectPlaceholderValues(templateToProcess.Text, finalTemplateValuesForPlaceholders);

        return injectedText;
    }
}