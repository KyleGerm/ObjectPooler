```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
12th Gen Intel Core i7-12800H 2.40GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.312
  [Host]      : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3
  VeryLongRun : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3

Job=VeryLongRun  InvocationCount=1  IterationCount=500  
LaunchCount=4  UnrollFactor=1  WarmupCount=30  

```
| Method                           | Mean          | Error       | StdDev       | Median        | Allocated |
|--------------------------------- |--------------:|------------:|-------------:|--------------:|----------:|
| TimeGetObject                    |      6.389 ns |   0.1300 ns |     1.756 ns |      6.775 ns |         - |
| TimeTryGetObject                 |      8.212 ns |   0.2272 ns |     3.053 ns |      6.150 ns |         - |
| TimeRequestMultiple_Alloc1000    | 12,570.262 ns | 213.8465 ns | 2,878.698 ns | 12,137.500 ns |    8024 B |
| TimeRequestMultiple_NonAlloc1000 |  9,978.726 ns |  65.6926 ns |   794.586 ns |  9,875.000 ns |         - |
| TimeRequestMultiple_Alloc100     |  1,165.533 ns |  30.7898 ns |   416.059 ns |  1,227.500 ns |     824 B |
| TimeRequestMultiple_NonAlloc100  |    931.179 ns |  23.4945 ns |   316.594 ns |    907.500 ns |         - |
| TimeRequestMultiple_Alloc10      |    125.471 ns |   3.3456 ns |    45.197 ns |    114.250 ns |     104 B |
| TimeRequestMultiple_NonAlloc10   |     87.147 ns |   2.3858 ns |    29.905 ns |     72.250 ns |         - |
| ReturnToPool_InIteration         |     11.622 ns |   0.2184 ns |     2.948 ns |     11.625 ns |         - |
| ReturnToPool_Extension           |     10.603 ns |   0.2032 ns |     2.619 ns |     10.800 ns |         - |
