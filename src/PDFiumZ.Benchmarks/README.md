# PDFiumZ Performance Benchmarks

This project contains comprehensive performance benchmarks for PDFiumZ using BenchmarkDotNet.

## Running Benchmarks

### Prerequisites
- .NET 10.0 SDK or later
- PDFium native libraries (included in PDFiumZ package)

### Run All Benchmarks

```bash
cd src/PDFiumZ.Benchmarks
dotnet run -c Release
```

**Important**: Always run benchmarks in Release mode for accurate results.

### Run Specific Benchmarks

You can filter benchmarks by name:

```bash
dotnet run -c Release --filter "*Load*"      # Run only loading benchmarks
dotnet run -c Release --filter "*Render*"    # Run only rendering benchmarks
dotnet run -c Release --filter "*Batch*"     # Run only batch operation benchmarks
```

## Benchmark Categories

### 1. Document Loading
- `LoadSmallPdf` - Load a 1-page PDF
- `LoadMediumPdf` - Load a 10-page PDF

### 2. Page Operations
- `GetPage` - Get a single page from document
- `GetPageWithProperties` - Get page and access properties
- `CreatePage` - Create a new blank page
- `GetMultiplePages` - Batch retrieve multiple pages
- `CreateMultiplePages` - Create 10 pages in sequence

### 3. Rendering
- `RenderPage72Dpi` - Render at default 72 DPI
- `RenderPage150Dpi` - Render at 150 DPI (higher quality)
- `RenderPage300Dpi` - Render at 300 DPI (print quality)

### 4. Text Operations
- `ExtractText` - Extract all text from a page
- `SearchText` - Search for text in a page

### 5. Document Manipulation
- `MergeDocuments` - Merge 3 PDF documents
- `SplitDocument` - Split a document into parts
- `RotatePages` - Rotate pages in a document

### 6. Save Operations
- `SaveSmallDocument` - Save a 1-page document
- `SaveMediumDocument` - Save a 10-page document

### 7. Content Creation
- `AddTextWithFont` - Add text content to a page
- `AddWatermark` - Add watermark to a document

### 8. Metadata Operations
- `AccessMetadata` - Access document metadata
- `AccessSecurityInfo` - Access security and permissions

### 9. Real-World Scenarios
- `CompleteWorkflow` - Load → Render → Save
- `DocumentProcessing` - Load → Modify → Save

## Understanding Results

BenchmarkDotNet provides detailed metrics:

- **Mean**: Average execution time
- **Error**: Half of 99.9% confidence interval
- **StdDev**: Standard deviation of all measurements
- **Rank**: Relative performance ranking
- **Gen0/Gen1/Gen2**: Garbage collection statistics
- **Allocated**: Total memory allocated

## Performance Tips

Based on benchmark results, consider these optimizations:

1. **Batch Operations**: Use `GetPages()` instead of individual `GetPage()` calls when retrieving multiple pages
2. **DPI Selection**: Choose appropriate DPI for your use case:
   - 72 DPI: Screen display (fastest)
   - 150 DPI: High-quality display
   - 300 DPI: Print quality (slowest)
3. **Document Caching**: Keep documents open if you need to access multiple pages
4. **Memory Management**: Dispose resources promptly to reduce GC pressure
5. **Async Operations**: Use async APIs for I/O-bound operations

## Benchmark Test Data

The benchmarks automatically create test PDFs:
- `small.pdf`: 1 page with text and shapes
- `medium.pdf`: 10 pages with content

Test files are created in `benchmark-temp/` and cleaned up after tests complete.

## Contributing

When adding new features to PDFiumZ, please add corresponding benchmarks to track performance impact.

### Adding a New Benchmark

1. Add a new method to `PdfBenchmarks.cs`
2. Annotate with `[Benchmark]` attribute
3. Add descriptive `Description` parameter
4. Follow existing naming patterns
5. Document the benchmark in this README

Example:

```csharp
[Benchmark(Description = "My new operation")]
public void MyNewOperation()
{
    // Benchmark code here
}
```

## Continuous Performance Monitoring

Consider running benchmarks:
- Before and after major changes
- Before releases
- When optimizing specific features
- To compare different approaches

## Notes

- Benchmarks run multiple iterations for statistical accuracy
- First run may be slower due to JIT compilation
- Results may vary based on:
  - CPU speed and available cores
  - Memory availability
  - Disk I/O performance
  - Background processes
  - PDFium library version

For most reliable results, run on a quiet system with minimal background activity.
