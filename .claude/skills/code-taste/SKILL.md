---
name: code-taste
description: Rules for function size, naming, and type-driven structure in any language — the recurring feedback given to authors. Load before writing or editing any code, and when reviewing a diff.
---

# Global Principles (Language-Agnostic)

These principles apply to **any programming language** as default rules for both **production code** and **tests**.

If a language-specific instruction conflicts with these, follow the more specific rule.

---

## Software Craftsmanship & SOLID Principles

- **Single Responsibility Principle (SRP)**: each class/function should have one clear responsibility.
- **Open/Closed Principle (OCP)**: be open for extension, closed for modification.
- **Liskov Substitution Principle (LSP)**: subtypes must be substitutable for their base types.
- **Interface Segregation Principle (ISP)**: prefer small, focused interfaces over large, general ones.
- **Dependency Inversion Principle (DIP)**: depend on abstractions, not concrete implementations.

- Write clean, readable, and maintainable code.
- Refactor regularly to improve design and readability.
- Favor composition over inheritance.
- Use meaningful names and keep functions/classes small.

- **KISS**: prefer the simplest solution that works; avoid unnecessary abstraction and cleverness.
- **YAGNI**: don’t build features, hooks, or generic extensibility until there’s a proven need.
- **Boy Scout Rule**: leave the codebase cleaner than you found it (small, safe improvements with each change).

---

## Testing Principles

Use `testing-first` skill for detailed testing guidance.

---

## DRY (Don't Repeat Yourself)

- Avoid duplication in production code and tests.
- Prefer:
    - factories/builders for test objects,
    - helper methods for recurring assertions,
    - parameterized tests for similar scenarios,
    - constants for repeated literals.

Balance matters:

- Don’t over-abstract.
- Some duplication is acceptable when it improves clarity.

---

## Core Guidelines

### 1) Production Code Style

- Idiomatic and aligned with modern language best practices (readability first).
- Prefer clear, simple code over clever code.
- Prefer **composition over inheritance**.
- Prefer **interfaces** for dependencies and inject dependencies via constructors.
- Use **meaningful names** : No name a reader would need to open the definition to disambiguate from something else in scope
- Case conventions:
    - Classes: `UpperCamelCase`
    - Methods/fields/variables: `lowerCamelCase`
    - Constants: `UPPER_SNAKE_CASE`
- Avoid side effects in getters and simple mappers.
- Keep methods small and single-purpose.

---

### 2) Imports & Code Style

- Use **explicit imports only** (no wildcard imports).
- Remove unused imports.
- Avoid fully qualified names in code; rely on imports.
- Prefer explicit lambda parameters for clarity.

---

### 3) Coverage & Edge Cases

Cover all logical branches, including:

- `null` inputs (if your API allows them) and null-handling behaviour.
- Empty collections.
- Boundary conditions (min/max values, size 0/1, etc.).
- Exception scenarios:
    - Dependency throws.
    - Validation failures.
    - Unexpected states.

---

## Language notes

General guidelines above take priority; these are where a language's idioms change how they cash out. Load the file matching the language being written or reviewed:

- Java → `languages/java.md`
- Kotlin → `languages/kotlin.md`