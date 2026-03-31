using System.Globalization;
using CrudeTemplate.TemplateExtensions;

namespace CrudeTemplate.Tests;

public class DateExtensionsTests
{
    [Fact]
    public void WithLocalizedDate_DefaultFormat_UsesShortDatePattern()
    {
        // Arrange
        DateTime date = new(2025, 3, 15);
        CultureInfo culture = CultureInfo.InvariantCulture;
        Template template = new("Date: {{Date}}");

        // Act
        template.WithLocalizedDate("Date", date, culture);
        string result = template.Render();

        // Assert
        string expected = $"Date: {date.ToString("d", culture)}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WithLocalizedDate_CustomFormat_FormatsCorrectly()
    {
        // Arrange
        DateTime date = new(2025, 12, 31);
        CultureInfo culture = CultureInfo.InvariantCulture;
        Template template = new("Date: {{Date}}");

        // Act
        template.WithLocalizedDate("Date", date, culture, "yyyy-MM-dd");
        string result = template.Render();

        // Assert
        Assert.Equal("Date: 2025-12-31", result);
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("en-US")]
    [InlineData("ja-JP")]
    public void WithLocalizedDate_DifferentCultures_FormatsAccordingToCulture(string cultureName)
    {
        // Arrange
        DateTime date = new(2025, 6, 1);
        CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
        Template template = new("Date: {{Date}}");

        // Act
        template.WithLocalizedDate("Date", date, culture);
        string result = template.Render();

        // Assert
        string expected = $"Date: {date.ToString("d", culture)}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WithLocalizedDate_NullTemplate_ThrowsArgumentNullException()
    {
        Template template = null!;

        Assert.Throws<ArgumentNullException>(() =>
            template.WithLocalizedDate("Key", DateTime.Now, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void WithLocalizedDate_NullCulture_ThrowsArgumentNullException()
    {
        Template template = new("{{Key}}");

        Assert.Throws<ArgumentNullException>(() =>
            template.WithLocalizedDate("Key", DateTime.Now, null!));
    }
}
