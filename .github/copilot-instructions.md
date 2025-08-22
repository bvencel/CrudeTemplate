# Contributing guidelines

This document provides essential guidelines for contributing to CrudeTemplate. For general C# style and naming, refer to the official [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) and [identifier naming rules](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names). Only project-specific requirements are listed below.

## Markdown documentation
- Use sentence case for headings and titles (e.g., "Contributing guidelines").

## Coding style
- Write code with clarity and simplicity. Avoid complex or convoluted logic.
- Use PascalCase for two-letter words (e.g., `AcVoltage`, `AiInputData`).
- Prefer explicit type declarations over `var`, except when the type is obvious or required.
- Never use `var` for types with 3-6 character names (e.g., always use `int`, `long`).
- Always use braces `{}` for all control blocks, even single-line statements.
- Write meaningful commit messages that clearly describe the intent and impact of the change.

## Naming conventions
- Use descriptive, unambiguous names for all identifiers.

## Code structure and readability
- Keep methods short and focused.
- Avoid magic numbers; use named constants or enums.
- Organize code logically within projects.
- Only 1 class/file.
- All helper classes go in Helpers folder. All extension methods go in Extensions folder. Base entities are stored with entities.

## Tooling and quality
- Use code organization tools (e.g., CodeMaid) to maintain consistency.
- Use analyzers and ensure the code compiles without any warnings.

## Nullability and type safety
- Use nullable reference types and annotate accordingly.
- Check for null arguments in public methods and throw `ArgumentNullException` as needed.

## Error handling
- Use exceptions for exceptional cases, not control flow.
- Catch only exceptions you can handle; avoid empty catch blocks.
- Log errors with enough context for troubleshooting.

## Testing
- Write unit tests for all new features and bug fixes.
- Use descriptive names for test methods.
- Ensure all tests pass before merging changes.

## Documentation
- Use XML documentation comments for public APIs.
- Keep documentation up to date with code changes.
- Document non-obvious design decisions or workarounds.
- Use Markdown for README files and other documentation.

---

By following these guidelines, we ensure a clean, consistent, and maintainable codebase for all contributors, including AI assistants. Thank you for helping make CrudeTemplate better!