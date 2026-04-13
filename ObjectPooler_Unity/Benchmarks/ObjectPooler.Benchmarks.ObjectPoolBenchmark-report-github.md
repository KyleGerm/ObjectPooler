```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
12th Gen Intel Core i7-12800H 2.40GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.312
  [Host]      : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3
  MediumRun   : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3
  VeryLongRun : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3

InvocationCount=1  UnrollFactor=1  

```

Job: VeryLongRun  
IterationCount: 500  
LaunchCount: 4  
WarmupCount: 30  


| Method                       |Mean          | Error       | StdDev        | Median        | Allocated |
|----------------------------- |-------------:|------------:|--------------:|--------------:|----------:|
| GetObject                    |     5.982 ns |   0.1302 ns |     1.7611 ns |      6.000 ns |         - |
| TryGetObject                 |     7.828 ns |   0.2268 ns |     3.0352 ns |      5.900 ns |         - |
| RequestMultiple_Alloc1000    |11,274.861 ns | 186.6102 ns | 2,514.6146 ns | 11,125.000 ns |    8024 B |
| RequestMultiple_NonAlloc1000 | 9,219.943 ns |  80.9441 ns |   999.2488 ns |  9,050.000 ns |         - |
| RequestMultiple_Alloc100     | 1,015.932 ns |  31.1541 ns |   421.9390 ns |  1,142.500 ns |     824 B |
| RequestMultiple_NonAlloc100  |   794.737 ns |  21.2937 ns |   287.1557 ns |    827.500 ns |         - |
| RequestMultiple_Alloc10      |   108.902 ns |   3.4706 ns |    46.8627 ns |     99.000 ns |     104 B |
| RequestMultiple_NonAlloc10   |    64.010 ns |   0.9923 ns |    11.9795 ns |     59.500 ns |         - |
| GetObjectManual_10x          |    91.452 ns |   1.5561 ns |    20.9795 ns |     89.000 ns |         - |

