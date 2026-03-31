using System.Globalization;
using CrudeTemplate.TemplateExtensions;

namespace CrudeTemplate.Tests;

public class CollectionExtensionsTests
{
    [Fact]
    public void WithJoinedItems_JoinsAndInjects()
    {
        // Arrange
        string[] items = ["apple", "banana", "cherry"];
        Template template = new("Items: {{List}}");

        // Act
        template.WithJoinedItems("List", items, ", ", item => item);
        string result = template.Render();

        // Assert
        Assert.Equal("Items: apple, banana, cherry", result);
    }

    [Fact]
    public void WithJoinedItems_EmptyCollection_InjectsEmptyString()
    {
        // Arrange
        string[] items = [];
        Template template = new("Items: {{List}}");

        // Act
        template.WithJoinedItems("List", items, ", ", item => item);
        string result = template.Render();

        // Assert
        Assert.Equal("Items: ", result);
    }

    [Fact]
    public void WithJoinedItems_SingleItem_NoSeparator()
    {
        // Arrange
        int[] items = [42];
        Template template = new("Value: {{Val}}");

        // Act
        template.WithJoinedItems("Val", items, "; ", item => item.ToString(CultureInfo.InvariantCulture));
        string result = template.Render();

        // Assert
        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public void WithJoinedItems_WithSelector_TransformsItems()
    {
        // Arrange
        int[] items = [1, 2, 3];
        Template template = new("Numbers: {{Nums}}");

        // Act
        template.WithJoinedItems("Nums", items, "-", item => (item * 10).ToString(CultureInfo.InvariantCulture));
        string result = template.Render();

        // Assert
        Assert.Equal("Numbers: 10-20-30", result);
    }

    [Fact]
    public void WithJoinedItems_NullTemplate_ThrowsArgumentNullException()
    {
        Template template = null!;

        Assert.Throws<ArgumentNullException>(() =>
            template.WithJoinedItems("Key", ["a"], ", ", item => item));
    }

    [Fact]
    public void WithJoinedItems_NullItems_ThrowsArgumentNullException()
    {
        Template template = new("{{Key}}");

        Assert.Throws<ArgumentNullException>(() =>
            template.WithJoinedItems<string>("Key", null!, ", ", item => item));
    }

    [Fact]
    public void WithJoinedItems_NullSeparator_ThrowsArgumentNullException()
    {
        Template template = new("{{Key}}");

        Assert.Throws<ArgumentNullException>(() =>
            template.WithJoinedItems("Key", ["a"], null!, item => item));
    }

    [Fact]
    public void WithJoinedItems_NullSelector_ThrowsArgumentNullException()
    {
        Template template = new("{{Key}}");

        Assert.Throws<ArgumentNullException>(() =>
            template.WithJoinedItems<string>("Key", ["a"], ", ", null!));
    }
}
