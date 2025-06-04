using System;

namespace Jsonata.Net.Native;

/// <summary>
/// Provides execution-scoped state for a single JSONata expression evaluation.
/// Ensures deterministic behavior for time-based and random functions.
/// This class is used internally by the framework and should not be instantiated directly.
/// </summary>
public sealed class QueryExecutionState
{
    private readonly Lazy<Random> random = new Lazy<Random>();
    private readonly DateTimeOffset now = DateTimeOffset.UtcNow;

    internal Random Random => this.random.Value;
    internal DateTimeOffset Now => this.now;
}
