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

    private static string Body => $"Your order {PlaceholderOrderId.AsPlaceholder()} has shipped.";

    private static string Email =>
        $"""
        Dear {PlaceholderName.AsPlaceholder()},

        {PlaceholderBody.AsPlaceholder()}

        {PlaceholderFooter.AsPlaceholder()}
        """;

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

    [Fact]
    public void With_AllowsNullChildTemplate_TreatedAsEmptyString()
    {
        Template template = new("Hello, {{Name}}!");
        template.With("Name", null);
        string result = template.Render();
        Assert.Equal("Hello, !", result);
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