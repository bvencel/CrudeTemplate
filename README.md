# CrudeTemplate

CrudeTemplate is a minimal, recursive string templating engine for .NET, designed to be:
- Side-effect-free where possible
- Deterministic (rendering is context-free and parameterless)
- Composable (templates can embed other templates)
- Reusable (templates and rendered results can be cached and reused)

## Core philosophies

- Composition over parsing: Build complex documents by snapping together small, cacheable template blocks (Header, Body, Footer) rather than writing monolithic strings
- Zero magic: No reflection, no background delegates, no runtime compilation and no embedded scripting languages. What you construct in C# is exactly what renders
- Structural determinism: Logic (like if/else forks or loops) belongs in your C# code to determine which templates to snap together, not inside the template text itself
- Two-phase rendering: Define your immutable template tree once, then render it thousands of times by passing in lightweight, render-time primitive dictionaries (perfect for mass emails or exact timestamps)

## Getting started

Add a project reference to the `CrudeTemplate` library (and optionally `CrudeTemplate.TemplateExtensions` for date/number formatting helpers).

## Basic usage

Placeholders are named tokens delimited by `{{` and `}}`. Child templates replace placeholders recursively.

```csharp
var name = new Template("John");
var body = new Template("Your order {{OrderId}} has shipped.")
    .With("OrderId", new Template("#1234"));
var footer = new Template("Kind regards,\nThe Company");

var email = new Template("Dear {{Name}},\n\n{{Body}}\n\n{{Footer}}")
    .With("Name", name)
    .With("Body", body)
    .With("Footer", footer);

string result = email.Render();
```

Produces:

    Dear John,

    Your order #1234 has shipped.

    Kind regards,
    The Company

The template tree looks like this:

```text
    Email
    ├── Text: "Dear {{Name}},\n\n{{Body}}\n\n{{Footer}}"
    ├── Name: [Template("John")]
    ├── Body: [Template("Your order {{OrderId}} has shipped.")]
    │   └── OrderId: [Template("#1234")]
    └── Footer: [Template("Kind regards,\nThe Company")]
```

### WithText shorthand

Use `WithText` when the child template is a plain string with no further placeholders:

```csharp
Template email = new Template("Hello, {{Name}}!")
    .WithText("Name", "Alice");

string result = email.Render(); // "Hello, Alice!"
```

## Two-phase rendering with primitives

Structural child templates are assembled once. Primitives are lightweight string values injected at render time, ideal for per-request data like timestamps or recipient names.

```csharp
// Phase 1: Build the structural tree (cacheable, reusable)
Template body = new("Your order {{OrderId}} shipped on {{ShipDate}}.");
Template email = new Template("Dear {{Recipient}},\n\n{{Body}}")
    .With("Body", body);

// Phase 2: Inject runtime primitives per render call
Dictionary<string, string> primitives = new()
{
    ["Recipient"] = "John",
    ["OrderId"] = "#1234",
    ["ShipDate"] = "2025-01-15"
};

string result = email.Render(primitives);
```

Structural child templates take precedence over primitives when the same placeholder name is used in both.

## Escaped placeholders

If you need literal `{{` or `}}` in the rendered output, wrap the delimiters with the escape character `Ø`:

```csharp
// Use Ø{{ and }}Ø to output literal delimiters
Template template = new("Use Ø{{Name}}Ø as a placeholder.");
string result = template.Render();
// result: "Use {{Name}} as a placeholder."
```

See `TemplateDelimiters` for the full set of escape constants.

## Cloning

`Template` implements `ICloneable`. Use `CloneTemplate()` to produce a deep copy that you can modify independently of the original:

```csharp
Template original = new Template("Hello, {{Name}}!")
    .WithText("Name", "Alice");

Template variant = original.CloneTemplate();
variant.WithText("Name", "Bob");

original.Render(); // "Hello, Alice!"
variant.Render();  // "Hello, Bob!"
```

## Template extensions

The `CrudeTemplate.TemplateExtensions` package provides formatting helpers for common types.

### Date formatting

```csharp
using CrudeTemplate.TemplateExtensions;

Template template = new Template("Date: {{Date}}")
    .WithLocalizedDate("Date", new DateTime(2025, 3, 15), CultureInfo.GetCultureInfo("de-DE"));
// Renders: "Date: 15.03.2025"

// With an explicit format string
Template template2 = new Template("Date: {{Date}}")
    .WithLocalizedDate("Date", new DateTime(2025, 12, 31), "yyyy-MM-dd", CultureInfo.InvariantCulture);
// Renders: "Date: 2025-12-31"
```

### Number and currency formatting

```csharp
using CrudeTemplate.TemplateExtensions;

Template invoice = new Template("Total: {{Amount}}")
    .WithCurrency("Amount", 1234.56m, CultureInfo.GetCultureInfo("en-US"));
// Renders: "Total: $1,234.56"

Template report = new Template("Value: {{Val}}")
    .WithNumber("Val", 1234567.89m, CultureInfo.InvariantCulture);
// Renders: "Value: 1,234,567.89"

// With a custom format
Template precise = new Template("Value: {{Val}}")
    .WithNumber("Val", 1234.567m, "N2", CultureInfo.InvariantCulture);
// Renders: "Value: 1,234.57"
```

## Mutability

- The `Template` class is mutable. You can change the `Text` property and modify the `ChildTemplates` dictionary after creation.
- The `With` and `WithText` methods return the same `Template` instance for fluent chaining.
- Use `CloneTemplate()` to create an independent deep copy when you need to branch variations.

## Best fit for

- Scenarios where structure is fixed and only content changes (e.g. email templates, reports, notifications).

## Limitations

Not ideal for highly dynamic, logic-driven or user-customizable templates.

What this library does **not** do:
- No IO (e.g. loading from files or resources)
- No scripting or embedded logic
- No control flow (if/else/for)

It's intentionally minimal.

The core logic recursively processes template trees, replacing placeholders with values and handling reserved characters.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on code style, testing and documentation.

![Tests](https://github.com/bvencel/CrudeTemplate/actions/workflows/dotnet.yml/badge.svg)
