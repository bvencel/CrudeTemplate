using CrudeTemplate.TemplateExtensions;

namespace CrudeTemplate.Tests;

public class LayoutExtensionsTests
{
    [Theory]
    [InlineData("Hi", 10, ' ', "Hi        ")]
    [InlineData("Hello", 5, ' ', "Hello")]
    [InlineData("AB", 6, '.', "AB....")]
    [InlineData("", 3, '-', "---")]
    public void WithPaddedRight_PadsCorrectly(string value, int totalWidth, char paddingChar, string expected)
    {
        // Arrange
        Template template = new("[{{Col}}]");

        // Act
        template.WithPaddedRight("Col", value, totalWidth, paddingChar);
        string result = template.Render();

        // Assert
        Assert.Equal($"[{expected}]", result);
    }

    [Fact]
    public void WithPaddedRight_DefaultPaddingChar_UsesSpace()
    {
        // Arrange
        Template template = new("[{{Col}}]");

        // Act
        template.WithPaddedRight("Col", "X", 4);
        string result = template.Render();

        // Assert
        Assert.Equal("[X   ]", result);
    }

    [Fact]
    public void WithPaddedRight_NullTemplate_ThrowsArgumentNullException()
    {
        Template template = null!;

        Assert.Throws<ArgumentNullException>(() =>
            template.WithPaddedRight("Key", "text", 10));
    }

    [Fact]
    public void WithPaddedRight_NullValue_ThrowsArgumentNullException()
    {
        Template template = new("{{Key}}");

        Assert.Throws<ArgumentNullException>(() =>
            template.WithPaddedRight("Key", null!, 10));
    }
}
