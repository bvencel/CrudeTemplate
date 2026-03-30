using CrudeTemplate.Constants;
using CrudeTemplate.Extensions;
using CrudeTemplate.Helpers;

namespace CrudeTemplate.Tests;

public class TemplateHelperTests
{
    /// <summary>
    /// Test cases for <see cref="TemplateHelper.ReplaceEscapedPlaceholdersIfNeeded"/>:
    /// empty string, no escape sequences, escaped start and end, consecutive escapes, mixed content.
    /// </summary>
    public static TheoryData<string, string> EscapeReplacementCases => new()
    {
        { "", "" },
        { "no escapes here", "no escapes here" },
        { TemplateDelimiters.EscapedStart + "Name" + TemplateDelimiters.EscapedEnd, "{{Name}}" },
        { "before " + TemplateDelimiters.EscapedStart + "X" + TemplateDelimiters.EscapedEnd + " after", "before {{X}} after" },
        { TemplateDelimiters.EscapedStart + TemplateDelimiters.EscapedStart, "{{{{" },
        { TemplateDelimiters.EscapedEnd + TemplateDelimiters.EscapedEnd, "}}}}" },
    };

    /// <summary>
    /// Test cases for <see cref="TemplateHelper.InjectPlaceholderValues"/>:
    /// empty dictionary, single replacement, multiple replacements, unmatched key, empty value, repeated placeholder, empty text.
    /// </summary>
    public static TheoryData<string, Dictionary<string, string>, string> ReplacementCases => new()
    {
        { "Hello, {{Name}}!", new Dictionary<string, string>(), "Hello, {{Name}}!" },
        { "Hello, {{Name}}!", new Dictionary<string, string> { ["Name"] = "World" }, "Hello, World!" },
        { "{{A}} and {{B}}", new Dictionary<string, string> { ["A"] = "1", ["B"] = "2" }, "1 and 2" },
        { "No placeholders", new Dictionary<string, string> { ["Key"] = "Value" }, "No placeholders" },
        { "", new Dictionary<string, string> { ["Key"] = "Value" }, "" },
        { "Hello, {{Name}}!", new Dictionary<string, string> { ["Name"] = "" }, "Hello, !" },
        { "{{X}} and {{X}}", new Dictionary<string, string> { ["X"] = "42" }, "42 and 42" },
    };

    [Fact]
    public void InjectPlaceholderValues_NullText_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TemplateHelper.InjectPlaceholderValues(null!, []));
    }

    [Fact]
    public void InjectPlaceholderValues_NullValueComponents_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TemplateHelper.InjectPlaceholderValues("text", null!));
    }

    [Theory]
    [MemberData(nameof(ReplacementCases))]
    public void InjectPlaceholderValues_WithComponents_ProducesExpectedResult(
        string text, Dictionary<string, string> components, string expected)
    {
        string result = TemplateHelper.InjectPlaceholderValues(text, components);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ReplaceEscapedPlaceholdersIfNeeded_NullText_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TemplateHelper.ReplaceEscapedPlaceholdersIfNeeded(null!));
    }

    [Theory]
    [MemberData(nameof(EscapeReplacementCases))]
    public void ReplaceEscapedPlaceholdersIfNeeded_ProcessesEscapedDelimiters(
        string input, string expected)
    {
        string result = TemplateHelper.ReplaceEscapedPlaceholdersIfNeeded(input);

        Assert.Equal(expected, result);
    }
}