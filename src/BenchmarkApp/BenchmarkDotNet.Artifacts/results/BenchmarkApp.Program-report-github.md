```

BenchmarkDotNet v0.14.0, Ubuntu 22.04.5 LTS (Jammy Jellyfish) WSL
13th Gen Intel Core i7-1365U, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.116
  [Host]     : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2


```
| Method        | Mean         | Error      | StdDev     | Gen0      | Gen1     | Allocated   |
|-------------- |-------------:|-----------:|-----------:|----------:|---------:|------------:|
| ProcessNative |     19.74 μs |   0.372 μs |   0.366 μs |    5.6763 |   0.2136 |    34.81 KB |
| ProcessJs     | 17,640.17 μs | 350.679 μs | 502.933 μs | 4125.0000 | 375.0000 | 25373.69 KB |
