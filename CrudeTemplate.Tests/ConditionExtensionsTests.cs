using CrudeTemplate.TemplateExtensions;

namespace CrudeTemplate.Tests;

public class ConditionExtensionsTests
{
    [Theory]
    [InlineData(true, "Yes", null, "Yes")]
    [InlineData(false, "Yes", null, "")]
    [InlineData(true, "Active", "Inactive", "Active")]
    [InlineData(false, "Active", "Inactive", "Inactive")]
    [InlineData(true, "", "Fallback", "")]
    [InlineData(false, "Fallback", "", "")]
    public void WithCondition_InjectsCorrectText(
        bool condition, string trueText, string? falseText, string expected)
    {
        // Arrange
        Template template = new("Status: {{Status}}");

        // Act
        template.WithCondition("Status", condition, trueText, falseText);
        string result = template.Render();

        // Assert
        Assert.Equal($"Status: {expected}", result);
    }

    [Fact]
    public void WithCondition_NullTemplate_ThrowsArgumentNullException()
    {
        Template template = null!;

        Assert.Throws<ArgumentNullException>(() =>
            template.WithCondition("Key", true, "text"));
    }
}
