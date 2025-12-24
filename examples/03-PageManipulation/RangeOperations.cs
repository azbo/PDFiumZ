using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PDFiumZ;
using PDFiumZ.HighLevel;

/// <summary>
/// Demonstrates PDFiumZ Range/Index support for page operations.
/// Requires .NET 8.0 or later to compile.
/// </summary>
public class RangeOperations
{
    public static async Task Main(string[] args)
    {
        // Initialize PDFium library
        PdfiumLibrary.Initialize();

        Console.WriteLine("=== PDFiumZ Range/Index Support Examples ===\n");

        // For demonstration, create a simple PDF or use an existing one
        // using var document = PdfDocument.Open("sample.pdf");

        Console.WriteLine("Range operations require a PDF file to demonstrate.");
        Console.WriteLine("Please update the 'sample.pdf' path in the code below.\n");

        // Uncomment to run with actual PDF:
        // await RunAllExamples();

        // Show what's possible
        ShowSyntaxExamples();

        PdfiumLibrary.Shutdown();
    }

    private static async Task RunAllExamples()
    {
        const string pdfPath = "sample.pdf";
        const string outputDir = "output";

        Directory.CreateDirectory(outputDir);

        using var document = PdfDocument.Open(pdfPath);

        Console.WriteLine($"Document has {document.PageCount} pages\n");

        // Example 1: Get first N pages
        Example1_GetFirstPages(document);

        // Example 2: Get last N pages
        Example2_GetLastPages(document);

        // Example 3: Process middle range
        Example3_MiddleRange(document, outputDir);

        // Example 4: Delete pages
        Example4_DeletePages(document, outputDir);

        // Example 5: Move pages
        Example5_MovePages(document, outputDir);

        // Example 6: Generate thumbnails for specific range
        await Example6_GenerateThumbnails(document, outputDir);
    }

    /// <summary>
    /// Example 1: Get first N pages using Range syntax.
    /// </summary>
    private static void Example1_GetFirstPages(PdfDocument document)
    {
        Console.WriteLine("Example 1: Get first 10 pages");

        // Traditional way (still works on all frameworks)
        var pages1 = document.GetPages(0, 10);

        // Modern way using Range (.NET 8+)
        var pages2 = document.GetPages(..10);

        // Both are equivalent
        Console.WriteLine($"  Got {pages1.Count()} pages\n");
    }

    /// <summary>
    /// Example 2: Get last N pages using Index syntax.
    /// </summary>
    private static void Example2_GetLastPages(PdfDocument document)
    {
        Console.WriteLine("Example 2: Get last 5 pages");

        // Traditional way
        var pages1 = document.GetPages(document.PageCount - 5, 5);

        // Modern way using Index (^5 means "5th from end")
        var pages2 = document.GetPages(^5..);

        // Both are equivalent
        Console.WriteLine($"  Got {pages2.Count()} pages\n");
    }

    /// <summary>
    /// Example 3: Get middle range of pages.
    /// </summary>
    private static void Example3_MiddleRange(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 3: Get pages 5-15");

        // Traditional: GetPages(5, 10) - start at 5, get 10 pages
        // Modern: 5..15 - from page 5 up to (but not including) page 15
        var pages = document.GetPages(5..15);

        Console.WriteLine($"  Got {pages.Count()} pages");

        // Save as a new document (convert Range to page indices)
        var (offset, length) = (5..15).GetOffsetAndLength(document.PageCount);
        var pageIndices = Enumerable.Range(offset, length).ToArray();
        using var extractedDoc = document.Split(pageIndices);
        string outputPath = Path.Combine(outputDir, "pages_5_to_15.pdf");
        extractedDoc.Save(outputPath);

        Console.WriteLine($"  Saved to {outputPath}\n");
    }

    /// <summary>
    /// Example 4: Delete pages using Range syntax.
    /// </summary>
    private static void Example4_DeletePages(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 4: Delete first 3 pages");

        // Create a copy for this example
        using var copy = PdfDocument.Open("sample.pdf");

        // Delete first 3 pages
        copy.DeletePages(..3);

        string outputPath = Path.Combine(outputDir, "without_first_3.pdf");
        copy.Save(outputPath);

        Console.WriteLine($"  Saved to {outputPath}\n");
    }

    /// <summary>
    /// Example 5: Move pages using Range syntax.
    /// </summary>
    private static void Example5_MovePages(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 5: Move last 5 pages to beginning");

        // Create a copy for this example
        using var copy = PdfDocument.Open("sample.pdf");

        // Move last 5 pages to position 0
        copy.MovePages(0, ^5..);

        string outputPath = Path.Combine(outputDir, "last_5_first.pdf");
        copy.Save(outputPath);

        Console.WriteLine($"  Saved to {outputPath}\n");
    }

    /// <summary>
    /// Example 6: Generate thumbnails for a range of pages.
    /// </summary>
    private static async Task Example6_GenerateThumbnails(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 6: Generate thumbnails for first 10 pages");

        // Using Range with ImageGenerationOptions
#if NET8_0_OR_GREATER
        var options = ImageGenerationOptions.ForRange(..10, document.PageCount);
#else
        var options = ImageGenerationOptions.ForRange(0, 10);
#endif

        string thumbDir = Path.Combine(outputDir, "thumbnails");
        Directory.CreateDirectory(thumbDir);

        int pageIndex = 0;
        await foreach (var image in document.GenerateImagesAsync(options))
        {
            string outputPath = Path.Combine(thumbDir, $"page_{pageIndex}.png");
            image.SaveAsPng(outputPath);
            image.Dispose();
            pageIndex++;
        }

        Console.WriteLine($"  Generated thumbnails in {thumbDir}\n");
    }

    /// <summary>
    /// Show various Range syntax examples without requiring a PDF.
    /// </summary>
    private static void ShowSyntaxExamples()
    {
        Console.WriteLine("=== Range Syntax Examples ===\n");

        Console.WriteLine("Getting Pages:");
        Console.WriteLine("  ..10      → First 10 pages");
        Console.WriteLine("  5..15     → Pages 5-14 (15 pages total)");
        Console.WriteLine("  ^5..      → Last 5 pages");
        Console.WriteLine("  ..^10     → All except last 10 pages");
        Console.WriteLine("  5..       → From page 5 to end");
        Console.WriteLine("  ..10      → From start to page 10");

        Console.WriteLine("\nDeleting Pages:");
        Console.WriteLine("  DeletePages(..3)      → Delete first 3 pages");
        Console.WriteLine("  DeletePages(^5..)     → Delete last 5 pages");
        Console.WriteLine("  DeletePages(10..20)   → Delete pages 10-19");

        Console.WriteLine("\nMoving Pages:");
        Console.WriteLine("  MovePages(0, ^5..)     → Move last 5 pages to beginning");
        Console.WriteLine("  MovePages(10, ..5)    → Move first 5 pages after page 10");
        Console.WriteLine("  MovePages(20, 5..15)  → Move pages 5-14 to position 20");

        Console.WriteLine("\nGenerating Images:");
#if NET8_0_OR_GREATER
        Console.WriteLine("  ForRange(..10, pageCount)    → First 10 pages");
        Console.WriteLine("  ForRange(5..15, pageCount)   → Pages 5-14");
        Console.WriteLine("  ForRange(^5.., pageCount)    → Last 5 pages");
#else
        Console.WriteLine("  (Range syntax requires .NET 8+)");
#endif

        Console.WriteLine("\n=== Comparison ===\n");

        Console.WriteLine("Traditional (all frameworks):");
        Console.WriteLine("  GetPages(0, 10)");
        Console.WriteLine("  GetPages(document.PageCount - 5, 5)");
        Console.WriteLine("  DeletePages(Enumerable.Range(0, 3).ToArray())");

        Console.WriteLine("\nModern (.NET 8+ only):");
        Console.WriteLine("  GetPages(..10)");
        Console.WriteLine("  GetPages(^5..)");
        Console.WriteLine("  DeletePages(..3)");

        Console.WriteLine("\n✨ Both styles are fully supported!");
        Console.WriteLine("   Use Range when it improves readability.\n");
    }
}

/* Example Output:

=== PDF/Index Support Examples ===

Document has 100 pages

Example 1: Get first 10 pages
  Got 10 pages

Example 2: Get last 5 pages
  Got 5 pages

Example 3: Get pages 5-15
  Got 10 pages
  Saved to output/pages_5_to_15.pdf

Example 4: Delete first 3 pages
  Saved to output/without_first_3.pdf

Example 5: Move last 5 pages to beginning
  Saved to output/last_5_first.pdf

Example 6: Generate thumbnails for first 10 pages
  Generated thumbnails in output/thumbnails

=== Range Syntax Examples ===

...

*/
