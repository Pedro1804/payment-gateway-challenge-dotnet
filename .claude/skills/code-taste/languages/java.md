# Java

- Prefer the Java standard library:
    - `Optional` for “may be missing” results (but don’t overuse it; don’t store as fields).
    - `java.time.*` for dates/times.
    - Streams where they improve clarity; avoid overly complex stream chains.

- Use **final by default**:
    - `final` local variables when possible.
    - Prefer immutable objects.