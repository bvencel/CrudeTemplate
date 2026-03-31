using BenchmarkDotNet.Attributes;
using CrudeTemplate;
using Microsoft.VSDiagnostics;

namespace BenchmarkSuite1;

[CPUUsageDiagnoser]
public class InjectPlaceholderBenchmarks
{
    private string smallText = default!;
    private Dictionary<string, string> smallValues = default!;
    private string mediumText = default!;
    private Dictionary<string, string> mediumValues = default!;
    private string largeText = default!;
    private Dictionary<string, string> largeValues = default!;
    [GlobalSetup]
    public void Setup()
    {
        // Small: 3 placeholders
        smallText = "Hello {{FirstName}} {{LastName}}, welcome to {{City}}!";
        smallValues = new Dictionary<string, string>
        {
            ["FirstName"] = "John",
            ["LastName"] = "Doe",
            ["City"] = "London"
        };
        // Medium: 10 placeholders in a paragraph
        mediumText = "Dear {{Title}} {{FirstName}} {{LastName}},\n" + "Your order {{OrderId}} placed on {{OrderDate}} is {{Status}}.\n" + "Shipping to {{Address}}, {{City}}, {{Country}}.\n" + "Thank you, {{CompanyName}}.";
        mediumValues = new Dictionary<string, string>
        {
            ["Title"] = "Mr.",
            ["FirstName"] = "John",
            ["LastName"] = "Doe",
            ["OrderId"] = "ORD-123456",
            ["OrderDate"] = "2025-01-15",
            ["Status"] = "Shipped",
            ["Address"] = "123 Main Street",
            ["City"] = "London",
            ["Country"] = "United Kingdom",
            ["CompanyName"] = "Acme Corp"
        };
        // Large: repeated block with 50 placeholders
        System.Text.StringBuilder builder = new();
        largeValues = [];
        
        for (int i = 0; i < 50; i++)
        {
            builder.Append($"Item {{{{Key{i}}}}} has value. ");
            largeValues[$"Key{i}"] = $"Value{i}";
        }

        largeText = builder.ToString();
    }

    [Benchmark]
    public string SinglePass_Small() => Template.InjectPlaceholderValues(smallText, smallValues);
    [Benchmark]
    public string Simple_Small() => Template.InjectPlaceholderValuesSimple(smallText, smallValues);
    [Benchmark]
    public string SinglePass_Medium() => Template.InjectPlaceholderValues(mediumText, mediumValues);
    [Benchmark]
    public string Simple_Medium() => Template.InjectPlaceholderValuesSimple(mediumText, mediumValues);
    [Benchmark]
    public string SinglePass_Large() => Template.InjectPlaceholderValues(largeText, largeValues);
    [Benchmark]
    public string Simple_Large() => Template.InjectPlaceholderValuesSimple(largeText, largeValues);
}