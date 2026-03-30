---
mode: agent
description: Review code for issues and suggest improvements
---

# Code review checklist

Review the selected code for the following concerns:

## Code quality
- [ ] Clear and simple logic (no convoluted code)
- [ ] Descriptive, unambiguous names
- [ ] No magic numbers (use named constants or enums)
- [ ] Methods are short and focused
- [ ] One class per file

## Style compliance
- [ ] Explicit type declarations (avoid `var` except when required)
- [ ] Braces `{}` for all control blocks
- [ ] No `#region` directives
- [ ] No underscore prefix on private fields
- [ ] Nested ifs instead of `else if`
- [ ] File-scoped namespaces

## Safety
- [ ] Nullable reference types used correctly
- [ ] Null argument checks in public methods
- [ ] Exceptions for exceptional cases only
- [ ] Input validation for external data

## Async (if applicable)
- [ ] `async`/`await` for I/O-bound operations
- [ ] Methods suffixed with `Async`
- [ ] `CancellationToken` passed through call chains
- [ ] No `.Result` or `.Wait()`

## Context
{{{ selection }}}

Provide specific, actionable feedback with code examples where helpful.
