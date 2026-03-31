using CrudeTemplate.Helpers;

namespace CrudeTemplate;

/// <summary>
/// Represents a recursive, composable string template that supports named placeholders and child templates.
/// <para>
/// <b>Placeholders</b> are named tokens in the template text, delimited by <c>{{</c> and <c>}}</c> (e.g., <c>{{Name}}</c>). Each placeholder can be associated with a <b>child template</b>, which is itself a <see cref="Template"/> instance. When rendering, placeholders are recursively replaced by the rendered output of their corresponding child templates.
/// </para>
/// <para>
/// This design enables the construction of complex, tree-structured templates where each node (template) can have its own text and a set of named child templates. The rendering process is deterministic and context-free: rendering a template always produces the same result given the same tree structure.
/// </para>
/// <para>
/// <b>Escaping:</b> To output literal placeholder delimiters, see the escaping documentation in <see cref="TemplateDelimiters"/>.
/// </para>
/// <para>
/// <b>Example:</b>
/// <code language="csharp">
/// var name = new Template("John");
/// var body = new Template("Your order {{OrderId}} has shipped.")
///     .With("OrderId", new Template("#1234"));
/// var footer = new Template("Kind regards,\nThe Company");
/// var email = new Template("Dear {{Name}},\n\n{{Body}}\n\n{{Footer}}")
///     .With("Name", name)
///     .With("Body", body)
///     .With("Footer", footer);
/// string result = email.Render();
/// </code>
/// The above will render to:
/// <code>
/// Dear John,
///
/// Your order #1234 has shipped.
///
/// Kind regards,
/// The Company
/// </code>
/// </para>
/// <para>
/// <b>Mutability:</b> The <see cref="Template"/> class is mutable. You can modify the <see cref="Text"/> property and the <see cref="ChildTemplates"/> dictionary after creation. The <see cref="With"/> method is provided for convenience and returns this instance with the specified child template added or replaced, but you may also manipulate the instance directly.
/// </para>
/// <para>
/// <b>Placeholder validation:</b> Placeholders must not be null, empty, or whitespace. An <see cref="ArgumentException"/> is thrown if a placeholder is null, empty, or consists only of whitespace. Null <c>childText</c> values are allowed and treated as empty strings.
/// </para>
/// </summary>
public class Template(string text, Dictionary<string, Template>? childTemplates = null)
{
    /// <summary>
    /// The maximum recursion depth allowed when rendering templates.
    /// Exceeding this limit indicates a likely circular reference in the template tree.
    /// </summary>
    private const int MaxRecursionDepth = 100;

    /// <summary>
    /// Gets or sets the dictionary of child templates (placeholders). Never null.
    /// Each key corresponds to a placeholder name in the template text, and the value is the <see cref="Template"/> to substitute for that placeholder.
    /// </summary>
    public Dictionary<string, Template> ChildTemplates
    {
        get => field;
        set => field = value ?? [];
    } = childTemplates ?? [];

    /// <summary>
    /// Gets or sets the template text. May contain placeholders (e.g., <c>{{Name}}</c>).
    /// </summary>
    public string Text { get; set; } = text ?? throw new ArgumentNullException(nameof(text));

    /// <summary>
    /// Renders this template and all its child templates recursively, replacing all placeholders.
    /// </summary>
    /// <returns>The fully rendered template string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the template tree exceeds the maximum recursion depth of <see cref="MaxRecursionDepth"/>, indicating a likely circular reference.</exception>
    public string Render()
    {
        string processed = RenderRecursively(this);
        processed = TemplateHelper.ReplaceEscapedPlaceholdersIfNeeded(processed);

        return processed;
    }

    /// <summary>
    /// Adds or replaces a child template for the given placeholder key. Placeholders must not be null, empty, or whitespace.
    /// </summary>
    /// <param name="placeholder">The name of the placeholder to associate with the child template. Must not be null, empty, or whitespace.</param>
    /// <param name="childTemplate">The <see cref="Template"/> to insert or replace for the specified placeholder. May be null (treated as an empty template).</param>
    /// <returns>This <see cref="Template"/> instance with the specified child template added or replaced.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="placeholder"/> is null, empty, or whitespace.</exception>
    public Template With(string placeholder, Template? childTemplate)
    {
        if (string.IsNullOrWhiteSpace(placeholder))
        {
            throw new ArgumentException("Placeholder must not be null, empty, or whitespace.", nameof(placeholder));
        }

        ChildTemplates[placeholder] = childTemplate ?? new Template(string.Empty);

        return this;
    }

    /// <summary>
    /// Adds or replaces a child template for the given placeholder key, using a string as the child template's text. Placeholders must not be null, empty, or whitespace. Null <paramref name="childText"/> is treated as an empty string.
    /// </summary>
    /// <param name="placeholder">The name of the placeholder to associate with the child template. Must not be null, empty, or whitespace.</param>
    /// <param name="childText">The text for the new child template. May be null (treated as empty string).</param>
    /// <returns>This <see cref="Template"/> instance with the specified child template added or replaced.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="placeholder"/> is null, empty, or whitespace.</exception>
    public Template WithText(string placeholder, string? childText)
    {
        return With(placeholder, new Template(childText ?? string.Empty));
    }

    /// <summary>
    /// Recursively renders the template and all its child templates, replacing placeholders with their rendered values.
    /// </summary>
    /// <param name="templateToProcess">The template to render.</param>
    /// <param name="depth">The current recursion depth, used to detect circular references.</param>
    /// <returns>The fully rendered template string, with all placeholders replaced by their corresponding child template output.</returns>
    /// <exception cref="InvalidOperationException">Thrown when recursion depth exceeds <see cref="MaxRecursionDepth"/>, indicating a likely circular reference.</exception>
    private static string RenderRecursively(Template templateToProcess, int depth = 0)
    {
        if (depth > MaxRecursionDepth)
        {
            throw new InvalidOperationException(
                $"Template recursion exceeded maximum depth of {MaxRecursionDepth}. This may indicate a circular reference in the template tree.");
        }

        Dictionary<string, string> finalTemplateValuesForPlaceholders = [];

        foreach (KeyValuePair<string, Template> component in templateToProcess.ChildTemplates)
        {
            string processedComponentText = RenderRecursively(component.Value, depth + 1);
            finalTemplateValuesForPlaceholders.Add(component.Key, processedComponentText);
        }

        return TemplateHelper.InjectPlaceholderValues(templateToProcess.Text, finalTemplateValuesForPlaceholders);
    }
}