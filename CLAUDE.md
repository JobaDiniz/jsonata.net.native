# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Jsonata.Net.Native is a .NET native implementation of the JSONata query and transformation language. It's a high-performance alternative to wrapping the JavaScript implementation, providing ~100x performance improvement. The project targets net8.0 and includes custom JSON parsing/DOM implementation for optimal performance.

## Code Style Guidelines

### C# Conventions
- Use sealed classes by default unless inheritance is explicitly needed
- Prefer record types for data transfer objects and immutable data structures
- Use C# 12+ features including primary constructors and required properties
- Follow standard .NET naming conventions (PascalCase for public members)
- Use camelCase for private fields and local variables. DO NOT use underscore prefix.
- Prefer dependency injection through constructor parameters

## Library Design

### Philosophy
Our libraries follow Microsoft's proven patterns for public API design, emphasizing:

- **Intuitive interfaces**: APIs should be self-documenting with minimal learning curve
- **Progressive disclosure**: Simple scenarios should be simple; complex scenarios possible
- **Consistency**: Similar concepts should have similar implementations across libraries
- **Extensibility**: Design for future extension without breaking changes

### API Surface Design
#### Namespace Organization

- Organize by feature area, not implementation details
- Keep public API surface minimal and focused
- Use consistent naming patterns across libraries

#### Interface Design

- Design interfaces for consumption first, implementation second
- Prefer small, focused interfaces over large, monolithic ones
- Follow the Interface Segregation Principle (ISP)
- Use fluent interfaces for configuration and builder patterns

#### Method Design

- Use method overloads for common scenarios
- Provide sensible defaults for optional parameters
- Return useful objects rather than primitive types
- Use consistent parameter ordering across similar methods

## Documentation

- XML documentation on all public APIs
- Include code examples for common scenarios
- Document edge cases and potential exceptions
- Provide migration guides for major version changes

## Development

### Building the Solution
```bash
dotnet build src/Jsonata.Net.Native.sln
```

### Running Tests
```bash
# Run all tests
dotnet test src/Jsonata.Net.Native.sln
```

## Testing

### Strategy
- **Unit Tests**: Core functionality tests in Jsonata.Net.Native.Tests
- **TestSuite**: Reference test suite from original JSONata-JS implementation
- **JSON Parser Tests**: Validation against JSONTestSuite and JSON_checker test sets
- **Integration Tests**: SystemTextJson binding tests

**TestSuite** are the more important tests. These are tests from the original JSONata-JS implementation.

There are some failing tests because not everything is implemented or supported yet. When running all the tests from the solution, this is the current output: Failed:   236, Passed:  1254, Skipped:   138, Total:  1628

You should use `Xunit` framework. You should strive to use `VerifyTests` snapshots for tests that produce complex output.