using CrudeTemplate.Constants;
using CrudeTemplate.Extensions;

namespace CrudeTemplate.Tests;

public class TemplateTests
{
    private const string Footer =
        """
        Kind regards,
        The Company
        """;

    private const string Name = "John";
    private const string OrderId = "#1234";
    private const string PlaceholderBody = "Body";
    private const string PlaceholderFooter = "Footer";
    private const string PlaceholderName = "Name";
    private const string PlaceholderOrderId = "OrderId";

    /// <summary>
    /// Test cases for <see cref="Template.ReplaceEscapedPlaceholdersIfNeeded"/>:
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
    /// Test cases for <see cref="Template.InjectPlaceholderValues"/>:
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

    private static string Body => $"Your order {PlaceholderOrderId.AsPlaceholder()} has shipped.";

    private static string Email =>
        $"""
        Dear {PlaceholderName.AsPlaceholder()},

        {PlaceholderBody.AsPlaceholder()}

        {PlaceholderFooter.AsPlaceholder()}
        """;

    [Theory]
    [InlineData("Name", "{{Name}}")]
    [InlineData("OrderId", "{{OrderId}}")]
    [InlineData("", "{{}}")]
    [InlineData("A B C", "{{A B C}}")]
    public void AsPlaceholder_WrapsKeyInDelimiters(string key, string expected)
    {
        string result = key.AsPlaceholder();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ChildTemplates_SetToNull_CoercesToEmptyDictionary()
    {
        Template template = new("text")
        {
            ChildTemplates = null!
        };

        Assert.NotNull(template.ChildTemplates);
        Assert.Empty(template.ChildTemplates);
    }

    [Fact]
    public void Constructor_NullChildTemplates_DefaultsToEmptyDictionary()
    {
        Template template = new("text", null);

        Assert.NotNull(template.ChildTemplates);
        Assert.Empty(template.ChildTemplates);
    }

    [Fact]
    public void Constructor_NullText_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Template(null!));
    }

    [Fact]
    public void Constructor_NullTextWithChildTemplates_ThrowsArgumentNullException()
    {
        Dictionary<string, Template> children = new() { ["Key"] = new Template("Value") };

        Assert.Throws<ArgumentNullException>(() => new Template(null!, children));
    }

    [Fact]
    public void InjectPlaceholderValues_NullText_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Template.InjectPlaceholderValues(null!, []));
    }

    [Fact]
    public void InjectPlaceholderValues_NullValueComponents_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Template.InjectPlaceholderValues("text", null!));
    }

    [Theory]
    [MemberData(nameof(ReplacementCases), DisableDiscoveryEnumeration = true)]
    public void InjectPlaceholderValues_WithComponents_ProducesExpectedResult(
        string text, Dictionary<string, string> components, string expected)
    {
        string result = Template.InjectPlaceholderValues(text, components);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Render_ChildTemplateOverridesPrimitiveForSameKey()
    {
        // Arrange: "Name" is both a child template and a primitive
        Template template = new Template("Hello, {{Name}}!")
            .WithText("Name", "Alice");

        Dictionary<string, string> primitives = new() { ["Name"] = "Bob" };

        // Act
        string result = template.Render(primitives);

        // Assert: structural child template takes precedence
        Assert.Equal("Hello, Alice!", result);
    }

    /// <summary>
    /// Verifies that a circular template reference throws <see cref="InvalidOperationException"/>
    /// instead of causing a stack overflow.
    /// </summary>
    [Fact]
    public void Render_CircularReference_ThrowsInvalidOperationException()
    {
        // Arrange: A references B, B references A
        Template templateA = new("{{B}}");
        Template templateB = new("{{A}}");
        templateA.With("B", templateB);
        templateB.With("A", templateA);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => templateA.Render());
    }

    [Fact]
    public void Render_ConsecutiveEscapedDelimiters_HandlesCorrectly()
    {
        string templateText = $"{TemplateDelimiters.EscapedStart}{TemplateDelimiters.EscapedStart}test{TemplateDelimiters.EscapedEnd}{TemplateDelimiters.EscapedEnd}";
        Template template = new(templateText);
        string result = template.Render();
        Assert.Equal("{{{{test}}}}", result);
    }

    /// <summary>
    /// Verifies deeply nested templates render correctly through multiple recursion levels.
    /// </summary>
    [Fact]
    public void Render_DeeplyNestedTemplates_ProducesExpectedOutput()
    {
        // Arrange: 4 levels deep
        Template innermost = new("world");
        Template level3 = new Template("{{Inner}}").With("Inner", innermost);
        Template level2 = new Template("{{Mid}}").With("Mid", level3);
        Template level1 = new Template("Hello, {{Outer}}!").With("Outer", level2);

        // Act
        string result = level1.Render();

        // Assert
        Assert.Equal("Hello, world!", result);
    }

    /// <summary>
    /// Verifies that the same placeholder used multiple times in the text is replaced in all occurrences.
    /// </summary>
    [Fact]
    public void Render_DuplicatePlaceholderInText_ReplacesAllOccurrences()
    {
        // Arrange
        string templateText = $"{"X".AsPlaceholder()} and {"X".AsPlaceholder()} and {"X".AsPlaceholder()}";
        Template template = new Template(templateText)
            .WithText("X", "42");

        // Act
        string result = template.Render();

        // Assert
        Assert.Equal("42 and 42 and 42", result);
    }

    /// <summary>
    /// Tests that rendering a template containing an escaped placeholder
    /// (using EscapedStart and EscapedEnd delimiters) produces the expected output
    /// where the placeholder is not replaced, but the delimiters are unescaped
    /// to the normal placeholder delimiters.
    /// </summary>
    [Fact]
    public void Render_EscapedPlaceholder_ProducesExpectedOutput()
    {
        // Arrange
        string templateText = $"Sentence start {TemplateDelimiters.EscapedStart}PlaceholderName{TemplateDelimiters.EscapedEnd} sentence end.";
        string expected = $"Sentence start {TemplateDelimiters.PlaceholderStart}PlaceholderName{TemplateDelimiters.PlaceholderEnd} sentence end.";

        Template template = new(templateText);

        // Act
        string result = template.Render();

        // Assert
        Assert.Equal(expected, result);
        Assert.DoesNotContain(TemplateDelimiters.EscapedStart, result);
        Assert.DoesNotContain(TemplateDelimiters.EscapedEnd, result);
    }

    /// <summary>
    /// Verifies that child templates with keys that do not match any placeholder in the text are silently ignored.
    /// </summary>
    [Fact]
    public void Render_ExtraChildTemplatesWithNoMatchingPlaceholder_AreIgnored()
    {
        // Arrange
        Template template = new Template("Hello, {{Name}}!")
            .WithText("Name", "Alice")
            .WithText("NonExistent", "Should be ignored");

        // Act
        string result = template.Render();

        // Assert
        Assert.Equal("Hello, Alice!", result);
        Assert.DoesNotContain("Should be ignored", result);
    }

    /// <summary>
    /// Tests that rendering a template with intermediary placeholders produces the expected output.
    /// This test verifies that:
    /// - Placeholders that are not replaced in the first rendering pass remain in the output.
    /// - A second rendering pass can replace those remaining placeholders.
    /// - All expected values are present in the final rendered result.
    /// - No unreplaced placeholders or delimiters remain in the output.
    /// </summary>
    [Fact]
    public void Render_IntermediaryResultsRenderedFurther_ProducesExpectedOutput()
    {
        // Arrange
        const string Subject = "Offer Accepted for Repair Request";
        const string RepairRequestId = "RR-2025-007";
        const string PlantName = "Solar Plant J";
        const string DeviceSerialNumber = "SN-123456789";
        const string NetAmount = "1200.50";
        const string TaxAmount = "228.10";
        const string FinalAmount = "1428.60";
        const string EuroSign = "€";

        Template requestDetailsLinkUrl = new($@"<a href='https://company.com/repairrequests/{"RepairRequestId".AsPlaceholder()}'>company.com/repairrequests/{"RepairRequestId".AsPlaceholder()}</a>");

        string emailTemplate =
            $"""
            Subject: {"Subject".AsPlaceholder()}

            Dear General Service,

            The offer for repair request {"RepairRequestId".AsPlaceholder()} at {"PlantName".AsPlaceholder()} (Device: {"DeviceSerialNumber".AsPlaceholder()}) has been accepted.

            Amounts:
            - Net: {"NetAmount".AsPlaceholder()} {"CurrencySign".AsPlaceholder()}
            - Tax: {"TaxAmount".AsPlaceholder()} {"CurrencySign".AsPlaceholder()}
            - Final: {"FinalAmount".AsPlaceholder()} {"CurrencySign".AsPlaceholder()}

            For more details, visit: {"RequestDetailsLink".AsPlaceholder()}
            """;

        Template intermedEmail = new Template(emailTemplate)
            .WithText("Subject", Subject)
            .WithText("PlantName", PlantName)
            .WithText("DeviceSerialNumber", DeviceSerialNumber)
            .WithText("CurrencySign", EuroSign)
            .WithText("NetAmount", NetAmount)
            .WithText("TaxAmount", TaxAmount)
            .WithText("FinalAmount", FinalAmount)
            .With("RequestDetailsLink", requestDetailsLinkUrl);

        // Act
        string intermedResult = intermedEmail.Render();

        Console.WriteLine($"Rendered email template: {intermedResult}");

        Assert.False(string.IsNullOrWhiteSpace(intermedResult), "Rendered email template should not be empty");
        Assert.Contains("RepairRequestId".AsPlaceholder(), intermedResult);

        Template email = new Template(intermedResult)
            .WithText("RepairRequestId", RepairRequestId);

        string result = email.Render();

        // Assert
        Assert.Contains(Subject, result);
        Assert.Contains(RepairRequestId, result);
        Assert.Contains(PlantName, result);
        Assert.Contains(DeviceSerialNumber, result);
        Assert.Contains(NetAmount, result);
        Assert.Contains(TaxAmount, result);
        Assert.Contains(FinalAmount, result);
        Assert.Contains(EuroSign, result);
        Assert.DoesNotContain(TemplateDelimiters.PlaceholderStart, result);
        Assert.DoesNotContain(TemplateDelimiters.PlaceholderEnd, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Hello, world!")]
    [InlineData("No placeholders here.")]
    public void Render_PlainTextWithoutPlaceholders_ReturnsOriginalText(string text)
    {
        Template template = new(text);

        string result = template.Render();

        Assert.Equal(text, result);
    }

    [Fact]
    public void Render_PrimitivesFlowIntoChildTemplates()
    {
        // Arrange: parent has no knowledge of "Sender", but the child does
        Template footer = new("Kind regards,\n{{Sender}}");
        Template email = new Template("{{Body}}\n\n{{Footer}}")
            .WithText("Body", "Some content.")
            .With("Footer", footer);

        Dictionary<string, string> primitives = new() { ["Sender"] = "ACME Corp" };

        // Act
        string result = email.Render(primitives);

        // Assert
        Assert.Contains("ACME Corp", result);
        Assert.DoesNotContain("{{Sender}}", result);
    }

    /// <summary>
    /// Creates and renders a template tree and asserts the output is non-empty and contains expected node and leaf text.
    /// </summary>
    [Fact]
    public void Render_SimpleTemplateTree_ProducesOutput()
    {
        // Arrange
        Template email = new Template(Email)
            .WithText(PlaceholderName, Name)
            .With(PlaceholderBody, new Template(Body)
                .WithText(PlaceholderOrderId, OrderId))
            .WithText(PlaceholderFooter, Footer);

        // Act
        string result = email.Render();

        Console.WriteLine($"Rendered template: {result}");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.Contains(Name, result);
        Assert.Contains(OrderId, result);
        Assert.DoesNotContain(TemplateDelimiters.PlaceholderStart, result);
        Assert.DoesNotContain(TemplateDelimiters.PlaceholderEnd, result);
        Assert.DoesNotContain(PlaceholderName, result);
        Assert.DoesNotContain(PlaceholderBody, result);
        Assert.DoesNotContain(PlaceholderOrderId, result);
        Assert.DoesNotContain(PlaceholderFooter, result);
    }

    [Fact]
    public void Render_TemplateWithWhitespaceValues_KeepsWhitespace()
    {
        // Arrange
        const string templateText = "Hello, {{Name}}!";
        const string whitespaceName = "      ";
        Template template = new Template(templateText)
            .With("Name", new Template(whitespaceName));

        // Act
        string result = template.Render();

        // Assert
        Assert.Contains(whitespaceName, result);
        Assert.Equal($"Hello,       !", result);
    }

    /// <summary>
    /// Verifies that placeholders with no matching child template are left verbatim in the output.
    /// </summary>
    [Fact]
    public void Render_UnmatchedPlaceholder_RemainsInOutput()
    {
        // Arrange
        string templateText = $"Hello, {"Name".AsPlaceholder()}! Your id is {"Id".AsPlaceholder()}.";
        Template template = new Template(templateText)
            .WithText("Name", "Alice");

        // Act
        string result = template.Render();

        // Assert
        Assert.Contains("Alice", result);
        Assert.Contains("Id".AsPlaceholder(), result);
    }

    [Fact]
    public void Render_WithEmptyPrimitives_BehavesSameAsParameterless()
    {
        // Arrange
        Template template = new Template("Hello, {{Name}}!")
            .WithText("Name", "Alice");

        // Act
        string withEmpty = template.Render(new Dictionary<string, string>());
        string withoutPrimitives = template.Render();

        // Assert
        Assert.Equal(withoutPrimitives, withEmpty);
    }

    [Fact]
    public void Render_WithPrimitives_InjectsRuntimeValues()
    {
        // Arrange: structure assembled once, primitives provided at render time
        Template body = new("Your order {{OrderId}} shipped on {{ShipDate}}.");
        Template email = new Template("Dear {{Recipient}},\n\n{{Body}}")
            .With("Body", body);

        Dictionary<string, string> primitives = new()
        {
            ["Recipient"] = "John",
            ["OrderId"] = "#1234",
            ["ShipDate"] = "2025-01-15"
        };

        // Act
        string result = email.Render(primitives);

        // Assert
        Assert.Contains("John", result);
        Assert.Contains("#1234", result);
        Assert.Contains("2025-01-15", result);
        Assert.DoesNotContain(TemplateDelimiters.PlaceholderStart, result);
    }

    [Fact]
    public void Render_WithPrimitives_NullDictionary_ThrowsArgumentNullException()
    {
        Template template = new("Hello");

        Assert.Throws<ArgumentNullException>(() => template.Render(null!));
    }

    [Fact]
    public void ReplaceEscapedPlaceholdersIfNeeded_NullText_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Template.ReplaceEscapedPlaceholdersIfNeeded(null!));
    }

    [Theory]
    [MemberData(nameof(EscapeReplacementCases))]
    public void ReplaceEscapedPlaceholdersIfNeeded_ProcessesEscapedDelimiters(
        string input, string expected)
    {
        string result = Template.ReplaceEscapedPlaceholdersIfNeeded(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void With_AllowsNullChildTemplate_TreatedAsEmptyString()
    {
        Template template = new("Hello, {{Name}}!");
        template.With("Name", null);
        string result = template.Render();
        Assert.Equal("Hello, !", result);
    }

    /// <summary>
    /// Verifies that calling <see cref="Template.With"/> twice for the same placeholder keeps only the last value.
    /// </summary>
    [Fact]
    public void With_SamePlaceholderTwice_OverwritesPreviousValue()
    {
        // Arrange
        Template template = new Template("Hello, {{Name}}!")
            .WithText("Name", "Alice")
            .WithText("Name", "Bob");

        // Act
        string result = template.Render();

        // Assert
        Assert.Equal("Hello, Bob!", result);
    }

    [Fact]
    public void With_ThrowsArgumentException_OnNullEmptyOrWhitespacePlaceholder()
    {
        Template template = new("Hello");

        Assert.Throws<ArgumentException>(() => template.WithText(null!, "value"));
        Assert.Throws<ArgumentException>(() => template.WithText(string.Empty, "value"));
        Assert.Throws<ArgumentException>(() => template.WithText("   ", "value"));

        Assert.Throws<ArgumentException>(() => template.With(null!, new Template("value")));
        Assert.Throws<ArgumentException>(() => template.With(string.Empty, new Template("value")));
        Assert.Throws<ArgumentException>(() => template.With("   ", new Template("value")));
    }

    [Fact]
    public void WithText_AllowsNullChildText_TreatedAsEmptyString()
    {
        Template template = new($"Hello, {"Name".AsPlaceholder()}!");
        template.WithText("Name", null);
        string result = template.Render();
        Assert.Equal("Hello, !", result);
    }
}