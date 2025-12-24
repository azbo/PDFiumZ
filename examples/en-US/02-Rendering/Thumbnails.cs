using PDFiumZ;
using PDFiumZ.HighLevel;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ThumbnailsExample;

/// <summary>
/// PDFiumZ Thumbnail Generation Example
/// Demonstrates how to generate various sizes and quality thumbnails for PDF documents
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== PDFiumZ Thumbnail Generation Example ===\n");

        // Check if PDF file exists
        const string samplePdf = "sample.pdf";
        if (!File.Exists(samplePdf))
        {
            Console.WriteLine($"Error: Sample PDF file '{samplePdf}' not found");
            Console.WriteLine("\nPlease rename a PDF file to 'sample.pdf' and place it in the current directory,");
            Console.WriteLine("or modify the file name in the code.");
            return;
        }

        // Initialize PDFium
        PdfiumLibrary.Initialize();

        try
        {
            using var document = PdfDocument.Open(samplePdf);
            Console.WriteLine($"Opened document: {samplePdf}");
            Console.WriteLine($"Total pages: {document.PageCount}\n");

            // ============================================
            // Example 1: Generate default thumbnails
            // ============================================
            Console.WriteLine("Example 1: Generate default thumbnails (200px, medium quality)");
            await Example1_DefaultThumbnails(document);
            Console.WriteLine();

            // ============================================
            // Example 2: Generate high-quality thumbnails
            // ============================================
            Console.WriteLine("Example 2: Generate high-quality thumbnails (400px)");
            await Example2_HighQualityThumbnails(document);
            Console.WriteLine();

            // ============================================
            // Example 3: Generate fast preview thumbnails
            // ============================================
            Console.WriteLine("Example 3: Generate fast preview thumbnails (150px, low quality)");
            await Example3_FastPreviews(document);
            Console.WriteLine();

            // ============================================
            // Example 4: Generate thumbnails for selected pages
            // ============================================
            Console.WriteLine("Example 4: Generate thumbnails for selected pages");
            await Example4_SelectedPagesThumbnails(document);
            Console.WriteLine();

            // ============================================
            // Example 5: Batch generate multiple sizes
            // ============================================
            Console.WriteLine("Example 5: Batch generate multiple size thumbnails");
            await Example5_MultipleSizes(document);
            Console.WriteLine();

            // ============================================
            // Example 6: Use ThumbnailOptions for fine control
            // ============================================
            Console.WriteLine("Example 6: Using ThumbnailOptions advanced configuration");
            await Example6_AdvancedOptions(document);
            Console.WriteLine();

            Console.WriteLine("=== All thumbnails generated successfully ===");
            Console.WriteLine("\nGenerated directories:");
            Console.WriteLine("  - thumbnails_default/  (default thumbnails)");
            Console.WriteLine("  - thumbnails_hq/       (high quality)");
            Console.WriteLine("  - thumbnails_fast/     (fast preview)");
            Console.WriteLine("  - thumbnails_selected/  (selected pages)");
            Console.WriteLine("  - thumbnails_multi/    (multiple sizes)");
            Console.WriteLine("  - thumbnails_custom/    (custom configuration)");
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
    /// Example 1: Generate default thumbnails
    /// </summary>
    private static async Task Example1_DefaultThumbnails(PdfDocument document)
    {
        string outputDir = "thumbnails_default";
        Directory.CreateDirectory(outputDir);

        // Use default ThumbnailOptions
        var options = ThumbnailOptions.Default;

        int pageIndex = 0;
        await Task.Run(() =>
        {
            foreach (var thumb in document.GenerateThumbnails(options))
            {
                using (thumb)
                {
                    string path = Path.Combine(outputDir, $"thumb_{pageIndex:D3}.png");
                    thumb.SaveAsPng(path);
                    Console.WriteLine($"  Generated: {Path.GetFileName(path)} ({thumb.Width}x{thumb.Height})");
                    pageIndex++;
                }
            }
        });

        Console.WriteLine($"  ✓ Generated {pageIndex} default thumbnails");
    }

    /// <summary>
    /// Example 2: Generate high-quality thumbnails
    /// </summary>
    private static async Task Example2_HighQualityThumbnails(PdfDocument document)
    {
        string outputDir = "thumbnails_hq";
        Directory.CreateDirectory(outputDir);

        // Large size + high quality
        var options = new ThumbnailOptions
        {
            MaxWidth = 400,
            Quality = 2  // 0=low, 1=medium, 2=high
        };

        int pageIndex = 0;
        await Task.Run(() =>
        {
            foreach (var thumb in document.GenerateThumbnails(options))
            {
                using (thumb)
                {
                    string path = Path.Combine(outputDir, $"thumb_hq_{pageIndex:D3}.png");
                    thumb.SaveAsPng(path);
                    pageIndex++;
                }
            }
        });

        Console.WriteLine($"  ✓ Generated {pageIndex} high-quality thumbnails (400px, highest quality)");
    }

    /// <summary>
    /// Example 3: Generate fast preview thumbnails
    /// </summary>
    private static async Task Example3_FastPreviews(PdfDocument document)
    {
        string outputDir = "thumbnails_fast";
        Directory.CreateDirectory(outputDir);

        // Small size + low quality (fastest)
        var options = ThumbnailOptions.Default
            .WithMaxWidth(150)
            .WithLowQuality();

        int pageIndex = 0;
        await Task.Run(() =>
        {
            foreach (var thumb in document.GenerateThumbnails(options))
            {
                using (thumb)
                {
                    string path = Path.Combine(outputDir, $"preview_{pageIndex:D3}.png");
                    thumb.SaveAsPng(path);
                    pageIndex++;
                }
            }
        });

        Console.WriteLine($"  ✓ Generated {pageIndex} fast previews (150px, low quality)");
    }

    /// <summary>
    /// Example 4: Generate thumbnails for selected pages
    /// </summary>
    private static async Task Example4_SelectedPagesThumbnails(PdfDocument document)
    {
        string outputDir = "thumbnails_selected";
        Directory.CreateDirectory(outputDir);

        // Generate thumbnails for first 5 pages and last page only
        var selectedPages = Enumerable.Range(0, Math.Min(5, document.PageCount)).ToList();
        if (document.PageCount > 5)
        {
            selectedPages.Add(document.PageCount - 1);
        }

        var options = new ThumbnailOptions
        {
            MaxWidth = 250,
            Quality = 1,
            PageIndices = selectedPages.ToArray()
        };

        int count = 0;
        await Task.Run(() =>
        {
            foreach (var thumb in document.GenerateThumbnails(options))
            {
                using (thumb)
                {
                    string path = Path.Combine(outputDir, $"thumb_page_{selectedPages[count]}.png");
                    thumb.SaveAsPng(path);
                    Console.WriteLine($"  Generated: thumb_page_{selectedPages[count]}.png");
                    count++;
                }
            }
        });

        Console.WriteLine($"  ✓ Generated {count} thumbnails for selected pages");
    }

    /// <summary>
    /// Example 5: Batch generate multiple sizes
    /// </summary>
    private static async Task Example5_MultipleSizes(PdfDocument document)
    {
        string baseDir = "thumbnails_multi";
        Directory.CreateDirectory(baseDir);

        // Define multiple sizes
        var sizes = new[]
        {
            (name: "small", width: 100),
            (name: "medium", width: 200),
            (name: "large", width: 400)
        };

        await Task.Run(() =>
        {
            foreach (var (name, width) in sizes)
            {
                string sizeDir = Path.Combine(baseDir, name);
                Directory.CreateDirectory(sizeDir);

                var options = new ThumbnailOptions
                {
                    MaxWidth = width,
                    Quality = 1
                };

                int pageIndex = 0;
                foreach (var thumb in document.GenerateThumbnails(options))
                {
                    using (thumb)
                    {
                        string path = Path.Combine(sizeDir, $"thumb_{pageIndex:D3}.png");
                        thumb.SaveAsPng(path);
                        pageIndex++;
                    }
                }

                Console.WriteLine($"  Generated {name} size ({width}px): {pageIndex} files");
            }
        });

        Console.WriteLine($"  ✓ Generated thumbnails in 3 different sizes");
    }

    /// <summary>
    /// Example 6: Use ThumbnailOptions advanced configuration
    /// </summary>
    private static async Task Example6_AdvancedOptions(PdfDocument document)
    {
        string outputDir = "thumbnails_custom";
        Directory.CreateDirectory(outputDir);

        // Configure using fluent API
        var options = ThumbnailOptions.Default
            .WithMaxWidth(300)
            .WithMediumQuality()
            .WithPages(new[] { 0, 1, document.PageCount - 1 });  // First and last pages

        Console.WriteLine("  Configuration:");
        Console.WriteLine($"    - Max width: 300px");
        Console.WriteLine($"    - Quality: medium");
        Console.WriteLine($"    - Pages: first page, second page, last page");

        int pageIndex = 0;
        await Task.Run(() =>
        {
            foreach (var thumb in document.GenerateThumbnails(options))
            {
                using (thumb)
                {
                    string path = Path.Combine(outputDir, $"custom_{pageIndex:D3}.png");
                    thumb.SaveAsPng(path);
                    Console.WriteLine($"  Generated: {Path.GetFileName(path)} ({thumb.Width}x{thumb.Height})");
                    pageIndex++;
                }
            }
        });

        Console.WriteLine($"  ✓ Generated {pageIndex} custom thumbnails using ThumbnailOptions");
    }
}
