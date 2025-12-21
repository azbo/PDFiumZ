using BenchmarkDotNet.Running;
using PDFiumZ.Benchmarks;

namespace PDFiumZ.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        // Run all benchmarks
        var summary = BenchmarkRunner.Run<PdfBenchmarks>();
    }
}
