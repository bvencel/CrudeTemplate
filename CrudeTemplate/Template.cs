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
public class Template
{
    private Dictionary<string, Template> _childTemplates = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Template"/> class with the specified text and optional child templates.
    /// </summary>
    /// <param name="text">The template text. Cannot be null. May contain placeholders (e.g., <c>{{Name}}</c>).</param>
    /// <param name="childTemplates">The child templates for placeholders. If null, an empty dictionary is used. Each key should match a placeholder name in <paramref name="text"/>.</param>
    public Template(string text, Dictionary<string, Template>? childTemplates)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
        ChildTemplates = childTemplates ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Template"/> class with the specified text and no child templates.
    /// </summary>
    /// <param name="text">The template text. Cannot be null.</param>
    public Template(string text)
        : this(text, null)
    {
    }

    /// <summary>
    /// Gets or sets the dictionary of child templates (placeholders). Never null.
    /// Each key corresponds to a placeholder name in the template text, and the value is the <see cref="Template"/> to substitute for that placeholder.
    /// </summary>
    public Dictionary<string, Template> ChildTemplates
    {
        get => _childTemplates;
        set => _childTemplates = value ?? [];
    }

    /// <summary>
    /// Gets or sets the template text. May contain placeholders (e.g., <c>{{Name}}</c>).
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Recursively renders the template and all its child templates, replacing placeholders with their rendered values.
    /// </summary>
    /// <param name="templateToProcess">The template to render. Cannot be null.</param>
    /// <returns>The fully rendered template string, with all placeholders replaced by their corresponding child template output.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="templateToProcess"/> is null.</exception>
    public static string RenderRecursively(Template templateToProcess)
    {
        ArgumentNullException.ThrowIfNull(templateToProcess);
        Dictionary<string, string> finalTemplateValuesForPlaceholders = [];

        if (templateToProcess.ChildTemplates.Count > 0)
        {
            foreach (KeyValuePair<string, Template> component in templateToProcess.ChildTemplates)
            {
                string processedComponentText = RenderRecursively(component.Value);
                finalTemplateValuesForPlaceholders.Add(component.Key, processedComponentText);
            }
        }

        return TemplateHelper.InjectPlaceholderValues(templateToProcess.Text, finalTemplateValuesForPlaceholders);
    }

    /// <summary>
    /// Renders this template and all its child templates recursively, replacing all placeholders.
    /// </summary>
    /// <returns>The fully rendered template string.</returns>
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
}