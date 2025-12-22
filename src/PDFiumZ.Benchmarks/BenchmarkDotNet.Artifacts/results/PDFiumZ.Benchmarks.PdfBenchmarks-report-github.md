```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.7462/25H2/2025Update/HudsonValley2)
Intel Core i9-14900HX 2.20GHz, 1 CPU, 32 logical and 24 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3


```
| Method                                      | Mean         | Error       | StdDev      | Rank | Gen0   | Allocated |
|-------------------------------------------- |-------------:|------------:|------------:|-----:|-------:|----------:|
| &#39;Load small PDF (1 page)&#39;                   |    52.053 μs |   0.2784 μs |   0.2604 μs |    3 |      - |     576 B |
| &#39;Load medium PDF (10 pages)&#39;                |    64.980 μs |   0.5701 μs |   0.5054 μs |    5 |      - |     584 B |
| &#39;Get single page&#39;                           |   545.104 μs |   3.5342 μs |   3.3059 μs |    9 |      - |     656 B |
| &#39;Get page and access properties&#39;            |   577.772 μs |   4.4737 μs |   3.7358 μs |   10 |      - |     656 B |
| &#39;Create new page&#39;                           |     4.682 μs |   0.0431 μs |   0.0403 μs |    1 |      - |     112 B |
| &#39;Render page at 72 DPI&#39;                     | 1,221.685 μs |  23.8762 μs |  24.5191 μs |   15 |      - |     944 B |
| &#39;Render page at 150 DPI&#39;                    | 2,657.685 μs |  39.5329 μs |  36.9791 μs |   17 |      - |    1024 B |
| &#39;Render page at 300 DPI&#39;                    | 8,663.995 μs | 169.5678 μs | 158.6138 μs |   19 |      - |    1024 B |
| &#39;Extract text from page&#39;                    |   606.271 μs |   6.8646 μs |   6.4211 μs |   11 |      - |     912 B |
| &#39;Search text in page&#39;                       |   617.754 μs |   8.8114 μs |   8.2421 μs |   11 |      - |    1480 B |
| &#39;Get multiple pages (batch)&#39;                | 1,143.475 μs |   4.2529 μs |   3.5513 μs |   14 |      - |    1232 B |
| &#39;Create 10 pages&#39;                           |    21.964 μs |   0.1704 μs |   0.1594 μs |    2 | 0.0305 |     800 B |
| &#39;Merge 3 documents&#39;                         |   299.078 μs |   3.4444 μs |   3.2219 μs |    7 |      - |    3200 B |
| &#39;Split document&#39;                            |   211.564 μs |   3.3865 μs |   3.1677 μs |    6 |      - |     760 B |
| &#39;Rotate all pages&#39;                          | 1,873.929 μs |  16.7324 μs |  14.8329 μs |   16 |      - |    1593 B |
| &#39;Save small document&#39;                       |   638.374 μs |  14.6213 μs |  40.0256 μs |   11 |      - |    7368 B |
| &#39;Save medium document&#39;                      | 1,065.325 μs |  20.5563 μs |  23.6727 μs |   13 |      - |    9664 B |
| &#39;Add text with font&#39;                        |   455.266 μs |   5.2186 μs |   4.8815 μs |    8 |      - |     616 B |
| &#39;Add watermark&#39;                             |   967.347 μs |   7.2259 μs |   6.7591 μs |   12 |      - |    1184 B |
| &#39;Access document metadata&#39;                  |    59.713 μs |   0.8794 μs |   0.6866 μs |    4 | 0.0610 |    2200 B |
| &#39;Access security info&#39;                      |    57.137 μs |   1.0349 μs |   0.9680 μs |    4 |      - |     576 B |
| &#39;Complete workflow: Load → Render → Save&#39;   | 2,527.937 μs | 107.3341 μs | 300.9763 μs |   17 |      - |    7681 B |
| &#39;Document processing: Load → Modify → Save&#39; | 6,479.268 μs | 128.6200 μs | 171.7040 μs |   18 |      - |   20861 B |
