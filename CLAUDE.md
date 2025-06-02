# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Jsonata.Net.Native is a .NET native implementation of the JSONata query and transformation language. It's a high-performance alternative to wrapping the JavaScript implementation, providing ~100x performance improvement. The project targets net8.0 and includes custom JSON parsing/DOM implementation for optimal performance.

## Build and Development Commands

### Building the Solution
```bash
dotnet build src/Jsonata.Net.Native.sln
```

### Running Tests
```bash
# Run all tests
dotnet test src/Jsonata.Net.Native.sln
```

## Architecture Overview

### Core Structure
- **Jsonata.Net.Native**: Main library with custom JSON parser/DOM and JSONata evaluation engine
- **Jsonata.Net.Native.SystemTextJson**: Binding package for System.Text.Json integration

### Key Components
- **Dom/**: AST node types for JSONata expressions (FieldNameNode, FunctionCallNode, etc.)
- **Eval/**: Evaluation engine including built-in functions and execution logic
- **Json/**: Custom JSON parser and DOM implementation (JToken, JObject, JArray, etc.)
- **Parsing/**: JSONata query parser (lexer, parser, tokens)

### Testing Strategy
- **Unit Tests**: Core functionality tests in Jsonata.Net.Native.Tests
- **TestSuite**: Reference test suite from original JSONata-JS implementation
- **JSON Parser Tests**: Validation against JSONTestSuite and JSON_checker test sets
- **Integration Tests**: SystemTextJson binding tests

**TestSuite** are the more important tests. These are tests from the original JSONata-JS implementation.

There are some failing tests because not everything is implemented or supported yet. When running all the tests from the solution, this is the current output: Failed:   236, Passed:  1254, Skipped:   138, Total:  1628

You should use `Xunit` framework. You should strive to use `VerifyTests` snapshots for tests that produce complex output.