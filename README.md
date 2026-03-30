# CrudeTemplate

CrudeTemplate is a minimal, recursive string templating engine for .NET, designed to be:
- Avoid side effects if possible
- Deterministic (rendering is context-free and parameterless)
- Composable (templates can embed other templates)
- Reusable (templates and rendered results can be cached and reused)

## Example

```C#
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

A tree like this

```text
    Email
    ├── Text: "Dear {{Name}},\n\n{{Body}}\n\n{{Footer}}"
    ├── Name: [Template("John")]
    ├── Body: [Template("Your order {{OrderId}} has shipped.")]
    │   └── OrderId: [Template("#1234")]
    └── Footer: [Template("Kind regards,\nThe Company")]
```

will render to a complete email, with all placeholders recursively filled in.


## Mutability

- The `Template` class is mutable. You can change the `Text` property and modify the `ChildTemplates` dictionary after creation.
- The `With` method is provided for convenience and returns the same `Template` instance with the specified child template added or replaced.

## Best fit for

- Scenarios where structure is fixed and only content changes.

## Limitations

Not ideal for highly dynamic, logic-driven or user-customizable templates.

What this library does **not** do
- No culture or localization support
- No IO (e.g. loading from files or resources)
- No scripting or embedded logic
- No runtime context or parameter injection
- No control flow (if/else/for)

It's intentionally minimal.

The core logic recursively processes template trees, replacing placeholders with values and handling reserved characters.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on code style, testing, and documentation.