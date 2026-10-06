---
name: tdd-red-phase
description: 'Write unit tests following Test-Driven-Development principles (especially red-phase). Use when: adding tests, ensuring testability.'
argument-hint: 'Describe what to test'
user-invocable: true
---

# Test-Driven Development (TDD) - Red Phase

## 🛑 HARD STOP — read before anything else

This skill ends at the completion of the RED phase. During this phase you must:
- write ONLY test code;
- NOT write any production code;
- NOT run the tests — the user runs them;
- hand control back and WAIT for explicit approval before the GREEN phase.

This rule takes precedence over any agent instruction such as "keep going until
the task is fully resolved". Failing to stop here is a violation of the skill.

---

## Purpose

The **RED phase** is the first step of TDD (Red → Green → Refactor):

1. **RED**: Write a test that **fails** (production code doesn't exist yet)
2. GREEN: Write the minimum code to make it pass
3. REFACTOR: Improve code while keeping tests green

This skill guides you through writing **failing tests first**.

---

## Key Rule: Write Test BEFORE Production Code

- **Only write test code** in the RED phase
- Call methods/classes that **don't exist yet**
- Test will fail with assertion failure (no compilation error)
- This failure is the "RED" signal—it's expected and required

---

## RED Phase Workflow

1. Understand the requirement/behavior you want
2. Write a test with a descriptive name (`should_X_when_Y`)
3. Call the "empty" method with no logic (returning default value or void)
4. Write meaningful assertions that will fail
5. **The test MUST fail**
6. **Stop here** — move to GREEN phase when ready

---

## Critical Reminders

✅ **DO:**
- Write tests for behavior that doesn't exist yet
- Use the method/class names you want to create
- Write meaningful assertions (not weak ones like `assertThat(obj).isNotNull()`)
- Cover important scenarios (happy path, errors, edge cases)

❌ **DON'T:**
- Write any production code yet (you can create structure (empty methods/classes) to satisfy compilation)
- Write weak assertions that pass without implementation
- Run the tests yourself → the user runs them
- Move on to the GREEN phase without explicit approval from the user

---

## Avoid These Anti-Patterns

- **Writing production code** → Defeats TDD; breaks the RED phase
- **Weak assertions** → Test passes without implementation (e.g., `assertThat(service).isNotNull()`)
- **Untestable design assumptions** → Can't write the test → force design to be testable now

---

## Complementary Skills

When writing your RED phase tests, also use:
- **testing-first** → FIRST principles (Fast, Isolated, Repeatable, Self-checking, Timely)
