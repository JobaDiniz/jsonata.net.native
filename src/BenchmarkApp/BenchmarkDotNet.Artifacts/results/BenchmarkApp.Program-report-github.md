```

BenchmarkDotNet v0.14.0, Ubuntu 22.04.5 LTS (Jammy Jellyfish) WSL
13th Gen Intel Core i7-1365U, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.116
  [Host]     : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2


```
| Method        | Mean         | Error      | StdDev     | Gen0      | Gen1     | Allocated   |
|-------------- |-------------:|-----------:|-----------:|----------:|---------:|------------:|
| ProcessNative |     20.78 μs |   0.392 μs |   0.367 μs |    5.6763 |   0.2136 |    34.81 KB |
| ProcessJs     | 18,363.38 μs | 284.877 μs | 237.885 μs | 4125.0000 | 375.0000 | 25373.69 KB |
