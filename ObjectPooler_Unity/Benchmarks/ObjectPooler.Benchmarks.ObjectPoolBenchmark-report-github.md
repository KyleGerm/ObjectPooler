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

| Method                       | Mean          | Error       | StdDev        | Median        | Allocated |
|----------------------------- |--------------:|------------:|--------------:|--------------:|----------:|
| GetObject                    |      4.353 ns |   0.0409 ns |     0.5294 ns |      4.175 ns |         - |
| TryGetObject                 |      4.664 ns |   0.0304 ns |     0.3662 ns |      4.600 ns |         - |
| RequestMultiple_Alloc1000    |  9,184.828 ns | 171.5521 ns | 2,312.8788 ns |  9,250.000 ns |    8024 B |
| RequestMultiple_NonAlloc1000 |  7,592.886 ns |  77.1601 ns |   981.2536 ns |  7,350.000 ns |         - |
| RequestMultiple_Alloc100     |    917.059 ns |  26.8763 ns |   363.2683 ns |  1,020.000 ns |     824 B |
| RequestMultiple_NonAlloc100  |    723.269 ns |  15.8180 ns |   213.5300 ns |    725.000 ns |         - |
| RequestMultiple_Alloc10      |    103.921 ns |   3.9183 ns |    52.6659 ns |     90.000 ns |     104 B |
| RequestMultiple_NonAlloc10   |     59.021 ns |   0.4801 ns |     5.7557 ns |     58.000 ns |         - |
| GetObjectManual_10x          |     81.918 ns |   1.1621 ns |    15.6203 ns |     75.500 ns |         - |

