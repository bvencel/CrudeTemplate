---
mode: agent
description: Generate xUnit tests for the selected code
---

# Generate xUnit tests

Create comprehensive unit tests for the selected code following these patterns:

## Requirements
- Use xUnit with `[Fact]` for single cases, `[Theory]` with `[InlineData]` for parameterized tests
- Follow Arrange-Act-Assert pattern
- Name tests as `MethodName_WhenCondition_ExpectedBehavior`
- Test happy paths and edge cases (null, empty, boundary values)
- Create helper methods for common test setup
- Use explicit type declarations (no `var`)
- Always use braces for control blocks

## Context
{{{ selection }}}

Generate tests in a new test class or add to an existing test class as appropriate.
