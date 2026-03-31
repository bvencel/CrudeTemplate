using System.Globalization;
using CrudeTemplate.TemplateExtensions;

namespace CrudeTemplate.Tests;

public class NumberExtensionsTests
{
    [Fact]
    public void WithCurrency_FormatsAsCurrency()
    {
        // Arrange
        CultureInfo culture = CultureInfo.GetCultureInfo("en-US");
        Template template = new("Total: {{Amount}}");

        // Act
        template.WithCurrency("Amount", 1234.56m, culture);
        string result = template.Render();

        // Assert
        string expected = $"Total: {1234.56m.ToString("C", culture)}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WithCurrency_ZeroAmount_FormatsCorrectly()
    {
        // Arrange
        CultureInfo culture = CultureInfo.InvariantCulture;
        Template template = new("Total: {{Amount}}");

        // Act
        template.WithCurrency("Amount", 0m, culture);
        string result = template.Render();

        // Assert
        string expected = $"Total: {0m.ToString("C", culture)}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WithNumber_DefaultFormat_UsesNumericFormat()
    {
        // Arrange
        CultureInfo culture = CultureInfo.InvariantCulture;
        Template template = new("Value: {{Val}}");

        // Act
        template.WithNumber("Val", 1234567.89m, culture);
        string result = template.Render();

        // Assert
        string expected = $"Value: {1234567.89m.ToString("N", culture)}";
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("N2", "1,234.57")]
    [InlineData("F0", "1235")]
    [InlineData("0.##", "1234.57")]
    public void WithNumber_CustomFormat_FormatsCorrectly(string format, string expectedValue)
    {
        // Arrange
        CultureInfo culture = CultureInfo.InvariantCulture;
        Template template = new("Value: {{Val}}");

        // Act
        template.WithNumber("Val", 1234.567m, format, culture);
        string result = template.Render();

        // Assert
        Assert.Equal($"Value: {expectedValue}", result);
    }

    [Fact]
    public void WithCurrency_NullTemplate_ThrowsArgumentNullException()
    {
        Template template = null!;

        Assert.Throws<ArgumentNullException>(() =>
            template.WithCurrency("Key", 1m, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void WithCurrency_NullCulture_ThrowsArgumentNullException()
    {
        Template template = new("{{Key}}");

        Assert.Throws<ArgumentNullException>(() =>
            template.WithCurrency("Key", 1m, null!));
    }

    [Fact]
    public void WithNumber_NullTemplate_ThrowsArgumentNullException()
    {
        Template template = null!;

        Assert.Throws<ArgumentNullException>(() =>
            template.WithNumber("Key", 1m, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void WithNumber_NullFormat_ThrowsArgumentNullException()
    {
        Template template = new("{{Key}}");

        Assert.Throws<ArgumentNullException>(() =>
            template.WithNumber("Key", 1m, null!, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void WithNumber_NullCulture_ThrowsArgumentNullException()
    {
        Template template = new("{{Key}}");

        Assert.Throws<ArgumentNullException>(() =>
            template.WithNumber("Key", 1m, "N", null!));
    }

    [Fact]
    public void WithNumber_NegativeValue_FormatsCorrectly()
    {
        // Arrange
        CultureInfo culture = CultureInfo.InvariantCulture;
        Template template = new("Value: {{Val}}");

        // Act
        template.WithNumber("Val", -999.99m, "N2", culture);
        string result = template.Render();

        // Assert
        string expected = $"Value: {(-999.99m).ToString("N2", culture)}";
        Assert.Equal(expected, result);
    }
}
