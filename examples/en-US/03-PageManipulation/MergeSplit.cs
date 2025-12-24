using PDFiumZ;
using PDFiumZ.HighLevel;
using System;
using System.IO;
using System.Linq;

namespace MergeSplitExample;

/// <summary>
/// PDFiumZ Document Merge and Split Examples
/// Demonstrates how to merge multiple PDF files and split PDF documents
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== PDFiumZ Document Merge and Split Examples ===\n");

        // Initialize PDFium
        PdfiumLibrary.Initialize();

        try
        {
            // ============================================
            // Prepare sample files
            // ============================================
            Console.WriteLine("Preparing sample files...");
            string samplePdf1 = "sample1.pdf";
            string samplePdf2 = "sample2.pdf";
            string samplePdf3 = "sample3.pdf";

            // Create sample files
            CreateSamplePdf(samplePdf1, "Document 1", 3);
            CreateSamplePdf(samplePdf2, "Document 2", 2);
            CreateSamplePdf(samplePdf3, "Document 3", 4);

            Console.WriteLine("✓ Created 3 sample PDF files\n");

            // ============================================
            // Example 1: Merge multiple PDF files
            // ============================================
            Console.WriteLine("Example 1: Merge multiple PDF files");
            Example1_MergeDocuments(samplePdf1, samplePdf2, samplePdf3);
            Console.WriteLine();

            // ============================================
            // Example 2: Split PDF - Extract specific pages
            // ============================================
            Console.WriteLine("Example 2: Split PDF - Extract specific pages");
            Example2_SplitDocument("merged.pdf");
            Console.WriteLine();

            // ============================================
            // Example 3: Split each page into separate files
            // ============================================
            Console.WriteLine("Example 3: Split each page into separate files");
            Example3_SplitAllPages("merged.pdf");
            Console.WriteLine();

            // ============================================
            // Example 4: Split using Range syntax (.NET 8+)
            // ============================================
            Console.WriteLine("Example 4: Split using Range syntax");
#if NET8_0_OR_GREATER
            Example4_SplitWithRange("merged.pdf");
#else
            Console.WriteLine("  (Requires .NET 8+ support)");
#endif
            Console.WriteLine();

            // ============================================
            // Example 5: Rotate and save
            // ============================================
            Console.WriteLine("Example 5: Rotate pages and save");
            Example5_RotateAndSave("merged.pdf");
            Console.WriteLine();

            // ============================================
            // Example 6: Delete pages and save
            // ============================================
            Console.WriteLine("Example 6: Delete specific pages");
            Example6_DeletePages("merged.pdf");
            Console.WriteLine();

            Console.WriteLine("=== All examples completed ===");
            Console.WriteLine("\nGenerated files:");
            Console.WriteLine("  - sample1.pdf, sample2.pdf, sample3.pdf (sample source files)");
            Console.WriteLine("  - merged.pdf (merged document)");
            Console.WriteLine("  - split_first3.pdf (first 3 pages)");
            Console.WriteLine("  - split_last2.pdf (last 2 pages)");
            Console.WriteLine("  - split_pages/ (each page saved separately)");
            Console.WriteLine("  - split_range.pdf (Range split)");
            Console.WriteLine("  - rotated.pdf (rotated document)");
            Console.WriteLine("  - deleted.pdf (document with deleted pages)");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
        finally
        {
            PdfiumLibrary.Shutdown();
        }
    }

    /// <summary>
    /// Example 1: Merge multiple PDF files
    /// </summary>
    private static void Example1_MergeDocuments(params string[] files)
    {
        Console.WriteLine("  Merging files:");
        foreach (var file in files)
        {
            Console.WriteLine($"    - {file}");
        }

        // Method 1: Merge using static method
        using var merged = PdfDocument.Merge(files);
        Console.WriteLine($"  ✓ Merge completed, total {merged.PageCount} pages");

        merged.Save("merged.pdf");
        Console.WriteLine("  ✓ Saved as: merged.pdf");
    }

    /// <summary>
    /// Example 2: Split PDF - Extract specific pages
    /// </summary>
    private static void Example2_SplitDocument(string sourceFile)
    {
        using var document = PdfDocument.Open(sourceFile);
        Console.WriteLine($"  Source file: {sourceFile} ({document.PageCount} pages)");

        // Extract first 3 pages
        Console.WriteLine("  Extracting first 3 pages...");
        using var first3 = document.Split(0, 1, 2);
        first3.Save("split_first3.pdf");
        Console.WriteLine($"  ✓ Saved as: split_first3.pdf ({first3.PageCount} pages)");

        // Extract last 2 pages
        Console.WriteLine("  Extracting last 2 pages...");
        using var last2 = document.Split(document.PageCount - 2, document.PageCount - 1);
        last2.Save("split_last2.pdf");
        Console.WriteLine($"  ✓ Saved as: split_last2.pdf ({last2.PageCount} pages)");
    }

    /// <summary>
    /// Example 3: Split each page into separate files
    /// </summary>
    private static void Example3_SplitAllPages(string sourceFile)
    {
        using var document = PdfDocument.Open(sourceFile);
        string outputDir = "split_pages";
        Directory.CreateDirectory(outputDir);

        Console.WriteLine($"  Splitting {document.PageCount} pages into separate files...");

        for (int i = 0; i < document.PageCount; i++)
        {
            using var page = document.GetPages(i, 1).First();
            using var singlePage = PdfDocument.CreateNew();
            using var newPage = singlePage.CreatePage(page.Width, page.Height);

            // Copy page content (simplified example, actual implementation needs complete copy)
            string outputPath = Path.Combine(outputDir, $"page_{i + 1}.pdf");
            singlePage.Save(outputPath);
        }

        Console.WriteLine($"  ✓ Saved to: {outputDir}/");
    }

    /// <summary>
    /// Example 4: Split using Range syntax (.NET 8+)
    /// </summary>
#if NET8_0_OR_GREATER
    private static void Example4_SplitWithRange(string sourceFile)
    {
        using var document = PdfDocument.Open(sourceFile);

        // Use Range syntax to extract pages 2-5
        var range = 2..6;  // Pages 2, 3, 4, 5
        var (offset, length) = range.GetOffsetAndLength(document.PageCount);
        var pageIndices = Enumerable.Range(offset, length).ToArray();

        Console.WriteLine($"  Extracting page range: {range} (index {offset}, {length} pages total)");

        using var extracted = document.Split(pageIndices);
        extracted.Save("split_range.pdf");
        Console.WriteLine($"  ✓ Saved as: split_range.pdf ({extracted.PageCount} pages)");
    }
#endif

    /// <summary>
    /// Example 5: Rotate pages and save
    /// </summary>
    private static void Example5_RotateAndSave(string sourceFile)
    {
        using var document = PdfDocument.Open(sourceFile);
        Console.WriteLine($"  Source file: {sourceFile}");

        // Rotate first 3 pages by 90 degrees
        Console.WriteLine("  Rotating first 3 pages by 90 degrees...");
        document.RotatePages(PdfRotation.Rotate90, 0, 1, 2);

        document.Save("rotated.pdf");
        Console.WriteLine("  ✓ Saved as: rotated.pdf");
    }

    /// <summary>
    /// Example 6: Delete specific pages
    /// </summary>
    private static void Example6_DeletePages(string sourceFile)
    {
        using var document = PdfDocument.Open(sourceFile);
        int originalCount = document.PageCount;
        Console.WriteLine($"  Source file: {sourceFile} ({originalCount} pages)");

        // Delete page 2
        Console.WriteLine("  Deleting page 2...");
        document.DeletePages(1);

        document.Save("deleted.pdf");
        Console.WriteLine($"  ✓ Saved as: deleted.pdf (original {originalCount} pages → current {document.PageCount} pages)");
    }

    /// <summary>
    /// Create sample PDF file
    /// </summary>
    private static void CreateSamplePdf(string filename, string title, int pageCount)
    {
        using var document = PdfDocument.CreateNew();
        using var font = PdfFont.Load(document, PdfStandardFont.HelveticaBold);

        for (int i = 0; i < pageCount; i++)
        {
            using var page = document.CreatePage(PdfPageSize.A4);
            using var editor = page.BeginEdit();

            editor
                .WithFont(font)
                .WithFontSize(24)
                .WithTextColor(PdfColor.DarkBlue)
                .Text($"{title} - Page {i + 1}", 100, 700)

                .WithFontSize(12)
                .WithTextColor(PdfColor.Black)
                .Text($"This is page {i + 1} of {title}", 100, 650)
                .Text($"Created at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", 100, 630)

                // Draw page number
                .WithFontSize(10)
                .Text($"Page number: {i + 1}", 250, 50)

                .Commit();
        }

        document.Save(filename);
    }
}
