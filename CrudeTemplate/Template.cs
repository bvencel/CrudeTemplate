using CrudeTemplate.Constants;
using CrudeTemplate.Extensions;

using System.Text;

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
    /// Replaces placeholders with values in the given text using a single-pass scan.
    /// </summary>
    /// <remarks>
    /// Scans the text once for <c>{{…}}</c> delimiters, looks up each placeholder name in the dictionary
    /// and emits the replacement value or the original placeholder token if no match is found.
    /// Unmatched opening delimiters (no corresponding closing delimiter) are emitted verbatim.
    /// </remarks>
    /// <param name="textWithPlaceholders">The text containing zero or more <c>{{Name}}</c>-style placeholders.</param>
    /// <param name="valueComponents">
    /// The replacement values keyed by raw placeholder name (e.g. <c>"Name"</c>, not <c>"{{Name}}"</c>).
    /// </param>
    /// <returns>The text with all matched placeholders replaced by their corresponding values.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="textWithPlaceholders"/> or <paramref name="valueComponents"/> is <see langword="null"/>.
    /// </exception>
    public static string InjectPlaceholderValues(string textWithPlaceholders, IDictionary<string, string> valueComponents)
    {
        /*
            Benchmarked this method against string.Replace (called "Simple" in the table below):

            | Method            | Mean         | Error      | StdDev     |
            |------------------ |-------------:|-----------:|-----------:|
            | SinglePass_Small  |     97.37 ns |   1.756 ns |   1.643 ns |
            | Simple_Small      |     86.39 ns |   1.668 ns |   1.479 ns |
            | SinglePass_Medium |    353.58 ns |   3.575 ns |   3.344 ns |
            | Simple_Medium     |    458.13 ns |   9.082 ns |   8.919 ns |
            | SinglePass_Large  |  1,676.70 ns |  32.165 ns |  30.087 ns |
            | Simple_Large      | 11,297.54 ns | 162.141 ns | 143.734 ns |

            - Using string.Replace wins only at very small scale (3 placeholders, ~11 ns advantage) because it avoids the StringBuilder overhead and performs just 3 string.Replace calls directly.
            - InjectPlaceholderValues(string, Dictionary<string, string>) (single-pass) dominates at medium and large scale. Its advantage grows dramatically with placeholder count because it scans the text exactly once, while the "Simple" version calls string.Replace in a loop — each call re-scans and reallocates the entire string, giving it O(n × m) behavior (n = text length, m = placeholder count).
            - Bottom line: InjectPlaceholderValues(string, Dictionary<string, string>) is the faster method for any realistic template size. The string.Replace variant only has a marginal edge for trivially small inputs.
        */

        ArgumentNullException.ThrowIfNull(textWithPlaceholders);
        ArgumentNullException.ThrowIfNull(valueComponents);

        // Nothing to replace – return early to avoid allocating a StringBuilder.
        if (valueComponents.Count == 0)
        {
            return textWithPlaceholders;
        }

        ReadOnlySpan<char> text = textWithPlaceholders.AsSpan();
        StringBuilder result = new(textWithPlaceholders.Length);

        // 'position' is the cursor that advances through the text exactly once (single-pass).
        int position = 0;

        while (position < text.Length)
        {
            // --- Step 1: Find the next opening delimiter "{{" from the current position. ---
            int startIndex = text[position..].IndexOf(TemplateDelimiters.PlaceholderStart);

            if (startIndex < 0)
            {
                // No more opening delimiters – append the remaining text and stop.
                _ = result.Append(text[position..]);

                break;
            }

            // Convert the relative index to an absolute index within the full text.
            startIndex += position;

            // Append everything between the cursor and the opening delimiter (literal text).
            _ = result.Append(text[position..startIndex]);

            // --- Step 2: Find the matching closing delimiter "}}" after the opening delimiter. ---
            int contentStart = startIndex + TemplateDelimiters.PlaceholderStart.Length;
            int endIndex = text[contentStart..].IndexOf(TemplateDelimiters.PlaceholderEnd);

            if (endIndex < 0)
            {
                // Opening delimiter with no closing counterpart – emit everything from "{{" onward as-is.
                _ = result.Append(text[startIndex..]);

                break;
            }

            // Convert the relative index to an absolute index.
            endIndex += contentStart;

            // --- Step 3: Extract the placeholder name between "{{" and "}}". ---
            string placeholderName = text[contentStart..endIndex].ToString();

            // --- Step 4: Replace the placeholder if a value exists; otherwise keep the original token. ---
            if (valueComponents.TryGetValue(placeholderName, out string? replacement))
            {
                _ = result.Append(replacement);
            }
            else
            {
                // No matching value – preserve the full "{{Name}}" token verbatim.
                _ = result.Append(text[startIndex..(endIndex + TemplateDelimiters.PlaceholderEnd.Length)]);
            }

            // Advance the cursor past the closing delimiter for the next iteration.
            position = endIndex + TemplateDelimiters.PlaceholderEnd.Length;
        }

        return result.ToString();
    }

    /// <summary>
    /// Replaces the placeholders with values in the given text using repeated <see cref="string.Replace(string, string)"/> calls.
    /// Less efficient than <see cref="InjectPlaceholderValues"/> for medium-to-large inputs. Kept for benchmarking comparison.
    /// </summary>
    /// <param name="textWithPlaceholders">The text with placeholders. Cannot be <see langword="null"/>.</param>
    /// <param name="valueComponents">The components to be replaced. Cannot be <see langword="null"/>.</param>
    /// <returns>The text with replaced placeholders.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="textWithPlaceholders"/> or <paramref name="valueComponents"/> is <see langword="null"/>.</exception>
    internal static string InjectPlaceholderValuesSimple(string textWithPlaceholders, Dictionary<string, string> valueComponents)
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
    /// Converts escaped placeholder delimiters to their literal characters.
    /// See <see cref="TemplateDelimiters"/> for escape sequence details.
    /// </summary>
    /// <example>
    /// <code language="csharp">
    /// // Input:  "Ø{{Name}}Ø"  →  Output: "{{Name}}"
    /// string result = Template.ReplaceEscapedPlaceholders("Ø{{Name}}Ø");
    /// </code>
    /// </example>
    /// <param name="textToBeProcessed">The text that needs processing. Cannot be <see langword="null"/>.</param>
    /// <returns>The text with escaped delimiters converted to literal delimiters.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="textToBeProcessed"/> is <see langword="null"/>.</exception>
    public static string ReplaceEscapedPlaceholders(string textToBeProcessed)
    {
        ArgumentNullException.ThrowIfNull(textToBeProcessed);

        textToBeProcessed = textToBeProcessed.Replace(TemplateDelimiters.EscapedStart, TemplateDelimiters.PlaceholderStart);
        textToBeProcessed = textToBeProcessed.Replace(TemplateDelimiters.EscapedEnd, TemplateDelimiters.PlaceholderEnd);

        return textToBeProcessed;
    }

    /// <summary>
    /// Creates a deep copy of this template, recursively cloning all child templates.
    /// </summary>
    /// <returns>A new, deeply cloned <see cref="Template"/> instance.</returns>
    public object Clone()
    {
        Template clonedTemplate = CloneTemplate();

        return clonedTemplate;
    }

    /// <summary>
    /// Creates a deep copy of this template, recursively cloning all child templates.
    /// Strongly typed for convenience.
    /// </summary>
    /// <returns>A new, deeply cloned <see cref="Template"/> instance.</returns>
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
    /// Renders this template and all its child templates recursively, replacing all structural placeholders.
    /// This method is purely functional and does not mutate the template instance.
    /// </summary>
    /// <returns>The fully rendered template string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the template tree exceeds the maximum recursion depth and indicating a likely circular reference.</exception>
    public string Render()
    {
        string processed = RenderRecursively(this, 0);
        processed = ReplaceEscapedPlaceholders(processed);

        return processed;
    }

    /// <summary>
    /// Renders this template and all its child templates recursively, replacing all placeholders with structure and runtime data.
    /// This method is purely functional and does not mutate the template instance.
    /// </summary>
    /// <param name="primitives">A dictionary of primitive string values to inject at render time (e.g. exact timestamps and names). Cannot be null.</param>
    /// <returns>The fully rendered template string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="primitives"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the template tree exceeds the maximum recursion depth and indicating a likely circular reference.</exception>
    public string Render(IDictionary<string, string> primitives)
    {
        ArgumentNullException.ThrowIfNull(primitives);

        string processed = RenderRecursively(this, 0);
        processed = InjectPlaceholderValues(processed, primitives);
        processed = ReplaceEscapedPlaceholders(processed);

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

        ChildTemplates[placeholder] = childTemplate ?? new Template(string.Empty);

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
        return With(placeholder, new Template(childText ?? string.Empty));
    }

    /// <summary>
    /// Recursively renders the template and all its child templates, replacing only structural placeholders.
    /// Primitives are not applied here; they are injected once after the full tree is resolved.
    /// </summary>
    /// <param name="templateToProcess">The template to process in the current recursion step.</param>
    /// <param name="depth">The current depth of recursion to prevent infinite loops.</param>
    /// <returns>The fully rendered string for the current template context.</returns>
    /// <exception cref="InvalidOperationException">Thrown when recursion depth exceeds <see cref="MaxRecursionDepth"/>.</exception>
    private static string RenderRecursively(Template templateToProcess, int depth)
    {
        if (depth > MaxRecursionDepth)
        {
            throw new InvalidOperationException($"Template recursion exceeded maximum depth of {MaxRecursionDepth}. This may indicate a circular reference in the template tree.");
        }

        Dictionary<string, string> finalTemplateValuesForPlaceholders = [];

        foreach (KeyValuePair<string, Template> childTemplatePair in templateToProcess.ChildTemplates)
        {
            string processedComponentText = RenderRecursively(childTemplatePair.Value, depth + 1);
            finalTemplateValuesForPlaceholders[childTemplatePair.Key] = processedComponentText;
        }

        string injectedText = InjectPlaceholderValues(templateToProcess.Text, finalTemplateValuesForPlaceholders);

        return injectedText;
    }
}