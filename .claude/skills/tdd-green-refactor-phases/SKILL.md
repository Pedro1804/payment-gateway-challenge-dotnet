---
name: tdd-green-refactor-phases
description: 'Write production code following Test-Driven-Development (GREEN & REFACTOR phases). Use when: implementing features, improving code quality while tests stay green.'
argument-hint: 'Describe what to implement or improve'
user-invocable: true
---

# Test-Driven Development (TDD) - GREEN & REFACTOR Phases

## Purpose

After RED phase (failing tests written), continue the TDD cycle:

1. RED: Write tests that fail ✅ (already done)
2. **GREEN**: Write minimal code to make tests pass
3. **REFACTOR**: Improve code while keeping tests green

This skill guides you through phases 2 & 3.

---

## GREEN Phase: Write Minimal Code

### Key Rule: Make Tests Pass—Nothing More, Nothing Less

- Write the **simplest possible code** to pass all failing tests
- Don't add features that aren't tested
- Don't optimize prematurely
- Focus: **Get to GREEN as fast as possible**

### GREEN Phase Workflow

1. Review the failing test(s)
2. Write the minimum production code needed
3. Run tests—they should **all PASS**
4. Move to REFACTOR phase (or stop if code is already clean)

---

## REFACTOR Phase: Clean Up Code

### Key Rule: Refactor While Tests Stay GREEN

- Tests must pass **before AND after** every refactor
- Improve readability, performance, design
- Remove duplication
- Simplify logic

### REFACTOR Cycle

1. Change one thing at a time (rename, extract method, etc.)
2. Run tests—they must stay **GREEN**
3. If tests fail: **undo and try differently**
4. Commit when tests pass
5. Repeat for next improvement

---

## Critical Reminders

✅ **DO:**
- Write the simplest code that passes tests (GREEN)
- Refactor **one small change at a time**
- Run tests after **every** change (REFACTOR)
- Move to REFACTOR only after all tests pass (GREEN)

❌ **DON'T:**
- Add untested features
- Refactor without running tests
- Skip the GREEN phase and jump to "perfect code"
- Ignore test failures during REFACTOR—stop immediately and fix

---

## Anti-Patterns to Avoid

- **Skipping GREEN phase** → Code becomes complex; hard to debug failures
- **Big refactors without running tests** → Tests fail; hard to find what broke
- **Adding features in REFACTOR** → Violates "refactor = same behavior"
- **Ignoring test failures** → Tests fail → undo changes → try again (don't ignore!)

---

## Complementary Skills

When implementing GREEN & REFACTOR phases, also use:
- **testing-first** → Ensure code quality aligns with FIRST principles

