---
name: testing-first
description: 'Write Java/Kotlin unit tests following FIRST principles (Fast, Isolated, Repeatable, Self-checking, Timely). Use when: adding tests, improving test coverage, reviewing test quality, ensuring testability.'
argument-hint: 'Describe what to test'
user-invocable: true
---

# Testing with FIRST Principles

## When to Use
- Writing new unit tests for Java/Kotlin code
- Improving existing test coverage
- Reviewing tests for quality issues
- Refactoring code to be more testable

## FIRST Principles Quick Checklist

### Fast ⚡
- Tests run in milliseconds
- **Action**: Avoid I/O, database calls, network requests
- **Action**: Use mocks/stubs for external dependencies
- **Action**: Keep test data minimal
- **Check**: Run suite completes in <5 seconds per 100 tests

### Isolated 🔒
- Each test is independent; no shared state
- **Action**: Use `@Before`/`@BeforeEach` to setup fresh state
- **Action**: Use `@After`/`@AfterEach` to clean up
- **Action**: Avoid static fields; use instance variables
- **Check**: Tests pass in any order, individually or together

### Repeatable 🔄
- Tests produce same result every run
- **Action**: Use `@ParameterizedTest` for multiple scenarios
- **Action**: Avoid hard-coded timestamps or random data
- **Action**: Use `LocalDate.of()` instead of `new Date()`
- **Check**: Run tests multiple times; they always pass

### Self-Checking ✅
- Test clearly passes or fails; no manual inspection needed
- **Action**: Use assertions: `assertEquals()`, `assertTrue()`, `assertThrows()`
- **Action**: One logical assertion per test (or grouped related assertions)
- **Action**: Avoid printing to console; use assertions instead
- **Check**: Test output shows exactly what failed (descriptive assertion messages)

### Timely ⏰
- Tests written before or with the implementation
- **Action**: Write test first (TDD) or immediately after code
- **Action**: Don't test deprecated code; update or remove
- **Action**: Keep tests in sync with production code
- **Check**: Test updates ship with code changes

### Also

- Each test must validate **one scenario**.
- Keep tests deterministic:
    - No real time, random, file system, network, or thread sleeps.
    - If time is involved, use an injected `Clock`.
- Avoiding direct tests of private methods (no reflection). Cover private logic via public APIs.

## Quick Test Template

```java
@Test
void should_return_true_when_input_is_valid() {
    // Arrange
    String input = "valid-input";
    
    // Act
    boolean result = myService.validate(input);
    
    // Assert
    assertTrue(result, "Expected validation to pass for valid input");
}
```

```kotlin
@Test
fun `should return aggregation details when aggregation exists`() {
    // Arrange

    // Act

    // Assert
}
```

- Don't forget to include // Arrange, Act, Assert comments to clarify the test structure :
  - Use `Arrange` to set up test data and mocks
  - Use `Act` to call the method/service being tested
  - Use `Assert` to verify the result matches expectations

## Versions & Frameworks

- Use JUnit 5 (Jupiter) for unit testing

- Use AssertJ fluent assertions for clarity:
    - `assertThat(actual).isEqualTo(expected)`
    - `assertThat(list).containsExactly(...)`
- Prefer **specific assertions**:
    - `isEmpty()`, `hasSize(...)`, `containsExactly(...)`, `containsExactlyInAnyOrder(...)`,
    - `isInstanceOf(...)`, `hasMessageContaining(...)`, etc.
- Keep assertions tight and expressive.

- Prefer Mockito explicit creation via `mock(...)`.
- Use `when(...).thenReturn(...)` for stubbing.
- Prefer `verify(...)` for behaviour verification.
- Prefer **strict, meaningful matchers**:
    - Use `argThat(...)` for complex matching.
    - Avoid mixing raw values and matchers in the same call.
- Use `ArgumentCaptor` when you need to assert on captured arguments.
- Mock only **collaborators** (dependencies). Don’t mock value objects.
- Avoid stubbing things you don’t use (keeps tests focused).

## Common Anti-Patterns to Avoid

| Anti-Pattern | Issue | Fix |
|---|---|---|
| `Thread.sleep()` in test | Slow, flaky | Use awaitility or mock the clock |
| Shared test state | Fails in isolation | Use `@BeforeEach`, not shared fields |
| No assertions | Test passes silently | Always assert the expected outcome |
| Sleeping instead of mocking | Slow | Mock external calls |
| Testing implementation details | Brittle | Test behavior, not internal state |

## Quick Workflow

1. **Arrange**: Set up test data and mocks
2. **Act**: Call the method/service being tested
3. **Assert**: Verify the result matches expectations
4. **Verify mocks** (if used): Check interactions if needed
5. **Run**: Execute and verify it passes
6. **Run again**: Verify it's repeatable
