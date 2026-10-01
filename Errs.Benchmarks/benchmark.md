## 0.1.0

```
BenchmarkDotNet v0.15.8, macOS Tahoe 26.7.1 (25G241) [Darwin 25.6.0]
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
```
| Method                       | Mean     | Error     | StdDev    |
|----------------------------- |---------:|----------:|----------:|
| TestNewErr                   | 4.770 μs | 0.0910 μs | 0.1118 μs |
| TestNewErrWithInnerException | 4.536 μs | 0.0278 μs | 0.0260 μs |
| TestIdentifyReasonWithSwitch | 4.528 μs | 0.0171 μs | 0.0143 μs |
| TestToString                 | 5.077 μs | 0.0780 μs | 0.0691 μs |
