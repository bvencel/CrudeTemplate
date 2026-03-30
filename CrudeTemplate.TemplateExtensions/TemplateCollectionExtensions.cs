namespace CrudeTemplate.TemplateExtensions;

/// <summary>
/// Extension methods for rendering collections into templates.
/// This is just a proof of concept.
/// </summary>
public static class TemplateCollectionExtensions
{
    /// <summary>
    /// Joins a collection of items into a single string using a separator and injects it into the template.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="template">The parent template. Cannot be null.</param>
    /// <param name="placeholder">The name of the placeholder.</param>
    /// <param name="items">The collection of items to join. Cannot be null.</param>
    /// <param name="separator">The string used to separate each item. Cannot be null.</param>
    /// <param name="selector">A function to extract a string representation from each item. Cannot be null.</param>
    /// <returns>This <see cref="Template"/> instance with the joined text added.</returns>
    public static Template WithJoinedItems<T>(
        this Template template,
        string placeholder,
        IEnumerable<T> items,
        string separator,
        Func<T, string> selector)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(separator);
        ArgumentNullException.ThrowIfNull(selector);

        string joinedText = string.Join(separator, items.Select(selector));
        return template.WithText(placeholder, joinedText);
    }
}