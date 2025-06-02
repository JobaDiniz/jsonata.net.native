# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Jsonata.Net.Native is a .NET native implementation of the JSONata query and transformation language. It's a high-performance alternative to wrapping the JavaScript implementation, providing ~100x performance improvement. The project targets multiple .NET frameworks (net47, netstandard2.0, net8.0) and includes custom JSON parsing/DOM implementation for optimal performance.

## Build and Development Commands

### Building the Solution
```bash
dotnet build src/Jsonata.Net.Native.sln
```

### Running Tests
```bash
# Run all tests
dotnet test src/Jsonata.Net.Native.sln

# Run specific test projects
dotnet test src/Jsonata.Net.Native.Tests/
dotnet test src/Jsonata.Net.Native.TestSuite/
dotnet test src/Jsonata.Net.Native.JsonNet.Tests/
dotnet test src/Jsonata.Net.Native.SystemTextJson.Tests/
dotnet test src/Jsonata.Net.Native.JsonParser.TestSuite/

# Run TestSuite with custom output (generates test reports)
dotnet test src/Jsonata.Net.Native.TestSuite/ --settings src/Jsonata.Net.Native.TestSuite/nunit.runsettings
```

### Running Example Applications
```bash
# Test application (basic usage examples)
dotnet run --project src/TestApp/

# Benchmark application (performance testing)
dotnet run --project src/BenchmarkApp/

# Windows Forms exerciser (interactive testing)
dotnet run --project src/JsonataExerciser/
```

### Packaging
```bash
# Build release packages
dotnet build src/Jsonata.Net.Native.sln -c Release
```

## Architecture Overview

### Core Structure
- **Jsonata.Net.Native**: Main library with custom JSON parser/DOM and JSONata evaluation engine
- **Jsonata.Net.Native.JsonNet**: Binding package for Newtonsoft.Json integration  
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
- **Integration Tests**: JsonNet and SystemTextJson binding tests

### Key Classes
- `JsonataQuery`: Main entry point for parsing and evaluating JSONata expressions
- `EvaluationEnvironment`: Context for variable and function bindings
- `JToken`: Base class for JSON DOM representation
- `EvalProcessor`: Core evaluation engine

### Test Framework
All test projects use NUnit 3.x with .NET 8.0 target framework. The TestSuite project includes custom NUnit settings for generating detailed test reports.

## Development Notes

### Custom JSON Implementation
Since v2.0.0, the library uses a custom JSON parser/DOM instead of external dependencies. This provides better performance and more JSONata-specific features.

### Multi-Framework Targeting
The main library targets .NET Framework 4.7, .NET Standard 2.0, and .NET 8.0 for broad compatibility.

### Strong Naming
Assemblies are signed with sgKey.snk for release builds to maintain assembly identity.