using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using PDFiumZ.HighLevel;

namespace PDFiumZ.Benchmarks;

/// <summary>
/// Performance benchmarks for PDFiumZ operations.
/// Tests key operations: document loading, page rendering, text extraction, and batch operations.
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class PdfBenchmarks
{
    private string _testPdfPath = null!;
    private string _smallPdfPath = null!;
    private string _mediumPdfPath = null!;
    private const int OperationIterations = 10;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Initialize PDFium library once
        PdfiumLibrary.Initialize();

        // Create test PDFs of various sizes
        Directory.CreateDirectory("benchmark-temp");

        // Small PDF: 1 page
        _smallPdfPath = Path.Combine("benchmark-temp", "small.pdf");
        CreateTestPdf(_smallPdfPath, 1);

        // Medium PDF: 10 pages
        _mediumPdfPath = Path.Combine("benchmark-temp", "medium.pdf");
        CreateTestPdf(_mediumPdfPath, 10);

        // Use existing sample PDF if available, otherwise create one
        _testPdfPath = File.Exists("pdf-sample.pdf")
            ? "pdf-sample.pdf"
            : _smallPdfPath;
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        // Cleanup
        PdfiumLibrary.Shutdown();

        if (Directory.Exists("benchmark-temp"))
        {
            Directory.Delete("benchmark-temp", true);
        }
    }

    /// <summary>
    /// Creates a test PDF with specified number of pages.
    /// </summary>
    private void CreateTestPdf(string path, int pageCount)
    {
        using var doc = PdfDocument.CreateNew();

        for (int i = 0; i < pageCount; i++)
        {
            using var page = doc.CreatePage(595, 842); // A4 size

            // Add some text content for more realistic testing
            var font = PdfFont.LoadStandardFont(doc, PdfStandardFont.Helvetica);
            using var editor = page.BeginEdit();

            editor.AddText($"Page {i + 1}", 50, 750, font, 24);
            editor.AddText($"This is test content for page {i + 1}.", 50, 700, font, 14);

            // Add some rectangles for visual content
            editor.AddRectangle(new PdfRectangle(50, 600, 100, 50), 0xFFFF0000, 0x80FF0000);
            editor.AddRectangle(new PdfRectangle(170, 600, 100, 50), 0xFF00FF00, 0x8000FF00);
            editor.AddRectangle(new PdfRectangle(290, 600, 100, 50), 0xFF0000FF, 0x800000FF);

            editor.GenerateContent();
            font.Dispose();
        }

        doc.SaveToFile(path);
    }

    // ===== Document Loading Benchmarks =====

    [Benchmark(Description = "Load small PDF (1 page)")]
    public void LoadSmallPdf()
    {
        using var doc = PdfDocument.Open(_smallPdfPath);
        _ = doc.PageCount; // Access a property to ensure document is fully loaded
    }

    [Benchmark(Description = "Load medium PDF (10 pages)")]
    public void LoadMediumPdf()
    {
        using var doc = PdfDocument.Open(_mediumPdfPath);
        _ = doc.PageCount;
    }

    // ===== Page Operations Benchmarks =====

    [Benchmark(Description = "Get single page")]
    public void GetPage()
    {
        using var doc = PdfDocument.Open(_testPdfPath);
        using var page = doc.GetPage(0);
        _ = page.Width; // Access a property
    }

    [Benchmark(Description = "Get page and access properties")]
    public void GetPageWithProperties()
    {
        using var doc = PdfDocument.Open(_testPdfPath);
        using var page = doc.GetPage(0);
        _ = page.Width;
        _ = page.Height;
        _ = page.Rotation;
    }

    [Benchmark(Description = "Create new page")]
    public void CreatePage()
    {
        using var doc = PdfDocument.CreateNew();
        using var page = doc.CreatePage(595, 842);
    }

    // ===== Rendering Benchmarks =====

    [Benchmark(Description = "Render page at 72 DPI")]
    public void RenderPage72Dpi()
    {
        using var doc = PdfDocument.Open(_testPdfPath);
        using var page = doc.GetPage(0);
        using var image = page.RenderToImage(); // Default 72 DPI
    }

    [Benchmark(Description = "Render page at 150 DPI")]
    public void RenderPage150Dpi()
    {
        using var doc = PdfDocument.Open(_testPdfPath);
        using var page = doc.GetPage(0);
        var options = RenderOptions.Default.WithDpi(150);
        using var image = page.RenderToImage(options);
    }

    [Benchmark(Description = "Render page at 300 DPI")]
    public void RenderPage300Dpi()
    {
        using var doc = PdfDocument.Open(_testPdfPath);
        using var page = doc.GetPage(0);
        var options = RenderOptions.Default.WithDpi(300);
        using var image = page.RenderToImage(options);
    }

    // ===== Text Operations Benchmarks =====

    [Benchmark(Description = "Extract text from page")]
    public void ExtractText()
    {
        using var doc = PdfDocument.Open(_testPdfPath);
        using var page = doc.GetPage(0);
        var text = page.ExtractText();
    }

    [Benchmark(Description = "Search text in page")]
    public void SearchText()
    {
        using var doc = PdfDocument.Open(_testPdfPath);
        using var page = doc.GetPage(0);
        var results = page.SearchText("Page", matchCase: false);
    }

    // ===== Batch Operations Benchmarks =====

    [Benchmark(Description = "Get multiple pages (batch)")]
    public void GetMultiplePages()
    {
        using var doc = PdfDocument.Open(_mediumPdfPath);
        var pages = doc.GetPages(0, Math.Min(5, doc.PageCount)).ToList();
        foreach (var page in pages)
        {
            page.Dispose();
        }
    }

    [Benchmark(Description = "Create 10 pages")]
    public void CreateMultiplePages()
    {
        using var doc = PdfDocument.CreateNew();
        for (int i = 0; i < 10; i++)
        {
            using var page = doc.CreatePage(595, 842);
        }
    }

    // ===== Document Manipulation Benchmarks =====

    [Benchmark(Description = "Merge 3 documents")]
    public void MergeDocuments()
    {
        using var merged = PdfDocument.Merge(_smallPdfPath, _smallPdfPath, _smallPdfPath);
    }

    [Benchmark(Description = "Split document")]
    public void SplitDocument()
    {
        using var doc = PdfDocument.Open(_mediumPdfPath);
        using var split = doc.Split(0, Math.Min(5, doc.PageCount));
    }

    [Benchmark(Description = "Rotate all pages")]
    public void RotateAllPages()
    {
        using var doc = PdfDocument.Open(_mediumPdfPath);
        doc.RotateAllPages(PdfRotation.Rotate90);
    }

    // ===== Save Operations Benchmarks =====

    [Benchmark(Description = "Save small document")]
    public void SaveSmallDocument()
    {
        using var doc = PdfDocument.Open(_smallPdfPath);
        var tempPath = Path.Combine("benchmark-temp", $"save-test-{Guid.NewGuid()}.pdf");
        doc.SaveToFile(tempPath);
        File.Delete(tempPath);
    }

    [Benchmark(Description = "Save medium document")]
    public void SaveMediumDocument()
    {
        using var doc = PdfDocument.Open(_mediumPdfPath);
        var tempPath = Path.Combine("benchmark-temp", $"save-test-{Guid.NewGuid()}.pdf");
        doc.SaveToFile(tempPath);
        File.Delete(tempPath);
    }

    // ===== Content Creation Benchmarks =====

    [Benchmark(Description = "Add text with font")]
    public void AddTextWithFont()
    {
        using var doc = PdfDocument.CreateNew();
        using var page = doc.CreatePage(595, 842);
        var font = PdfFont.LoadStandardFont(doc, PdfStandardFont.Helvetica);

        using (var editor = page.BeginEdit())
        {
            editor.AddText("Test Text", 50, 750, font, 14);
            editor.GenerateContent();
        }

        font.Dispose();
    }

    [Benchmark(Description = "Add watermark")]
    public void AddWatermark()
    {
        using var doc = PdfDocument.Open(_smallPdfPath);
        doc.AddTextWatermark(
            "CONFIDENTIAL",
            WatermarkPosition.Center,
            new WatermarkOptions { Opacity = 0.3, Rotation = 45 });
    }

    // ===== Metadata Operations Benchmarks =====

    [Benchmark(Description = "Access document metadata")]
    public void AccessMetadata()
    {
        using var doc = PdfDocument.Open(_testPdfPath);
        var meta = doc.Metadata;
        _ = meta.Title;
        _ = meta.Author;
        _ = meta.Producer;
        _ = meta.CreationDate;
    }

    [Benchmark(Description = "Access security info")]
    public void AccessSecurityInfo()
    {
        using var doc = PdfDocument.Open(_testPdfPath);
        var security = doc.Security;
        _ = security.IsEncrypted;
        _ = security.CanPrint;
        _ = security.CanModifyContents;
    }

    // ===== Real-World Scenario Benchmarks =====

    [Benchmark(Description = "Complete workflow: Load → Render → Save")]
    public void CompleteWorkflow()
    {
        var tempPath = Path.Combine("benchmark-temp", $"workflow-{Guid.NewGuid()}.pdf");

        using (var doc = PdfDocument.Open(_smallPdfPath))
        {
            using var page = doc.GetPage(0);
            using var image = page.RenderToImage();

            doc.SaveToFile(tempPath);
        }

        File.Delete(tempPath);
    }

    [Benchmark(Description = "Document processing: Load → Modify → Save")]
    public void DocumentProcessing()
    {
        var tempPath = Path.Combine("benchmark-temp", $"processing-{Guid.NewGuid()}.pdf");

        using (var doc = PdfDocument.Open(_mediumPdfPath))
        {
            // Simulate typical document processing
            doc.InsertBlankPage(0, 595, 842);
            doc.RotatePages(PdfRotation.Rotate90, 0);
            doc.AddTextWatermark("PROCESSED", WatermarkPosition.Center);

            doc.SaveToFile(tempPath);
        }

        File.Delete(tempPath);
    }
}
