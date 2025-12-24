using System;
using System.IO;
using System.Threading.Tasks;
using PDFiumZ;
using PDFiumZ.HighLevel;

/// <summary>
/// Demonstrates the new Options classes for PDF image operations:
/// - ImageGenerationOptions: Control which pages to render and how
/// - ImageSaveOptions: Control saving images to files
/// - ThumbnailOptions: Generate thumbnail images
/// </summary>
public class OptionsConfig
{
    public static async Task Main(string[] args)
    {
        // Initialize PDFium library
        PdfiumLibrary.Initialize();

        Console.WriteLine("=== PDFiumZ Options Classes Examples ===\n");

        // For demonstration, create a simple PDF or use an existing one
        // using var document = PdfDocument.Open("sample.pdf");

        Console.WriteLine("Options examples require a PDF file to demonstrate.");
        Console.WriteLine("Please update the 'sample.pdf' path in the code below.\n");

        // Uncomment to run with actual PDF:
        // await RunAllExamples();

        // Show what's possible
        ShowOptionsOverview();

        PdfiumLibrary.Shutdown();
    }

    private static async Task RunAllExamples()
    {
        const string pdfPath = "sample.pdf";
        const string outputDir = "output";

        Directory.CreateDirectory(outputDir);

        using var document = PdfDocument.Open(pdfPath);

        Console.WriteLine($"Document has {document.PageCount} pages\n");

        // ImageGenerationOptions Examples
        await Example1_DefaultOptions(document);
        await Example2_SpecificPages(document, outputDir);
        await Example3_PageRange(document, outputDir);
        await Example4_WithDpi(document, outputDir);
        await Example5_WithTransparency(document, outputDir);
        await Example6_ChainedOptions(document, outputDir);
#if NET8_0_OR_GREATER
        await Example7_RangeSyntax(document, outputDir);
#endif

        // ImageSaveOptions Examples
        await Example8_SaveWithDefaults(document, outputDir);
        await Example9_SaveWithCustomPattern(document, outputDir);
        await Example10_SaveWithPathGenerator(document, outputDir);

        // ThumbnailOptions Examples
        await Example11_DefaultThumbnails(document, outputDir);
        await Example12_CustomThumbnailSize(document, outputDir);
        await Example13_ThumbnailQuality(document, outputDir);
    }

    #region ImageGenerationOptions Examples

    /// <summary>
    /// Example 1: Default options - render all pages with default settings
    /// </summary>
    private static async Task Example1_DefaultOptions(PdfDocument document)
    {
        Console.WriteLine("Example 1: Default Options");

        // Using Default constant
        var options = ImageGenerationOptions.Default;

        int pageCount = 0;
        foreach (var image in document.GenerateImages(options))
        {
            using (image)
            {
                Console.WriteLine($"  Generated page {pageCount}: {image.Width}x{image.Height}");
                pageCount++;
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Example 2: Render specific pages by indices
    /// </summary>
    private static async Task Example2_SpecificPages(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 2: Specific Pages (0, 2, 5, 10)");

        // Render only specific pages
        var options = ImageGenerationOptions.ForPages(new[] { 0, 2, 5, 10 });

        int pageIndex = 0;
        foreach (var image in document.GenerateImages(options))
        {
            using (image)
            {
                string outputPath = Path.Combine(outputDir, $"specific_page_{image.Width}x{image.Height}.png");
                image.SaveAsPng(outputPath);
                Console.WriteLine($"  Saved: {outputPath}");
                pageIndex++;
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Example 3: Render a range of pages
    /// </summary>
    private static async Task Example3_PageRange(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 3: Page Range (pages 5-15)");

        // Render pages 5-15 (10 pages total)
        var options = ImageGenerationOptions.ForRange(startIndex: 5, count: 10);

        int count = 0;
        foreach (var image in document.GenerateImages(options))
        {
            using (image)
            {
                count++;
            }
        }
        Console.WriteLine($"  Generated {count} images");
        Console.WriteLine();
    }

    /// <summary>
    /// Example 4: High-DPI rendering
    /// </summary>
    private static async Task Example4_WithDpi(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 4: High-DPI (300 DPI)");

        // Create options with custom DPI
        var options = new ImageGenerationOptions
        {
            StartIndex = 0,
            Count = 1, // Just first page for demo
            RenderOptions = RenderOptions.Default.WithDpi(300)
        };

        foreach (var image in document.GenerateImages(options))
        {
            using (image)
            {
                string outputPath = Path.Combine(outputDir, "highres_page.png");
                image.SaveAsPng(outputPath);
                Console.WriteLine($"  Saved 300 DPI image: {image.Width}x{image.Height}");
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Example 5: Transparent background
    /// </summary>
    private static async Task Example5_WithTransparency(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 5: Transparent Background");

        // Create options with transparency
        var options = new ImageGenerationOptions
        {
            StartIndex = 0,
            Count = 1,
            RenderOptions = RenderOptions.Default.WithTransparency()
        };

        foreach (var image in document.GenerateImages(options))
        {
            using (image)
            {
                string outputPath = Path.Combine(outputDir, "transparent_page.png");
                image.SaveAsPng(outputPath);
                Console.WriteLine($"  Saved transparent PNG");
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Example 6: Method chaining for fluent configuration
    /// </summary>
    private static async Task Example6_ChainedOptions(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 6: Chained Options Configuration");

        // Using fluent API
        var options = new ImageGenerationOptions()
            .WithPages(new[] { 0, 1, 2 })
            .WithDpi(150)
            .WithTransparency();

        int pageCount = 0;
        foreach (var image in document.GenerateImages(options))
        {
            using (image)
            {
                Console.WriteLine($"  Generated page {pageCount} at 150 DPI with transparency");
                pageCount++;
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Example 7: Range syntax (.NET 8+ only)
    /// </summary>
#if NET8_0_OR_GREATER
    private static async Task Example7_RangeSyntax(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 7: Range Syntax (.NET 8+)");

        // First 10 pages
        var options1 = ImageGenerationOptions.ForRange(..10, document.PageCount);
        Console.WriteLine($"  First 10 pages: ..10");

        // Pages 5-15
        var options2 = ImageGenerationOptions.ForRange(5..15, document.PageCount);
        Console.WriteLine($"  Pages 5-14: 5..15");

        // Last 5 pages
        var options3 = ImageGenerationOptions.ForRange(^5.., document.PageCount);
        Console.WriteLine($"  Last 5 pages: ^5..");

        Console.WriteLine();
    }
#endif

    #endregion

    #region ImageSaveOptions Examples

    /// <summary>
    /// Example 8: Save with default settings
    /// </summary>
    private static async Task Example8_SaveWithDefaults(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 8: Save Images with Defaults");

        // Create output directory
        string saveDir = Path.Combine(outputDir, "saved_images");
        Directory.CreateDirectory(saveDir);

        // Save all pages with default options
        var options = ImageSaveOptions.ForAllPages(saveDir);

        string[] paths = document.SaveAsImages(options);
        Console.WriteLine($"  Saved {paths.Length} images to {saveDir}");
        Console.WriteLine($"  Pattern: page-0.png, page-1.png, ...");
        Console.WriteLine();
    }

    /// <summary>
    /// Example 9: Save with custom filename pattern
    /// </summary>
    private static async Task Example9_SaveWithCustomPattern(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 9: Custom Filename Pattern");

        string saveDir = Path.Combine(outputDir, "custom_names");
        Directory.CreateDirectory(saveDir);

        // Use custom filename pattern
        var options = ImageSaveOptions.ForAllPages(saveDir)
            .WithFileNamePattern("document_page_{0:D3}.png")
            .WithDpi(150);

        string[] paths = document.SaveAsImages(options);
        Console.WriteLine($"  Pattern: document_page_000.png, document_page_001.png, ...");
        Console.WriteLine();
    }

    /// <summary>
    /// Example 10: Save with custom path generator
    /// </summary>
    private static async Task Example10_SaveWithPathGenerator(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 10: Custom Path Generator");

        // Use custom path generator for full control
        var options = ImageSaveOptions.WithPathGenerator(
            pathGenerator: pageIndex => Path.Combine(outputDir, "custom", $"img_{pageIndex:000}.png"),
            startIndex: 0,
            count: 3 // Just first 3 pages
        );

        Directory.CreateDirectory(Path.Combine(outputDir, "custom"));
        string[] paths = document.SaveAsImages(options);
        Console.WriteLine($"  Saved to: output/custom/img_000.png, img_001.png, img_002.png");
        Console.WriteLine();
    }

    #endregion

    #region ThumbnailOptions Examples

    /// <summary>
    /// Example 11: Default thumbnail settings
    /// </summary>
    private static async Task Example11_DefaultThumbnails(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 11: Default Thumbnails");

        string thumbDir = Path.Combine(outputDir, "thumbnails");
        Directory.CreateDirectory(thumbDir);

        // Use default thumbnail options (200px max width, medium quality)
        var options = ThumbnailOptions.Default;

        int pageIndex = 0;
        foreach (var thumb in document.GenerateThumbnails(options))
        {
            using (thumb)
            {
                string path = Path.Combine(thumbDir, $"thumb_{pageIndex}.png");
                thumb.SaveAsPng(path);
                Console.WriteLine($"  Thumbnail {pageIndex}: {thumb.Width}x{thumb.Height}");
                pageIndex++;
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Example 12: Custom thumbnail size
    /// </summary>
    private static async Task Example12_CustomThumbnailSize(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 12: Large Thumbnails (400px)");

        string thumbDir = Path.Combine(outputDir, "large_thumbs");
        Directory.CreateDirectory(thumbDir);

        // Create larger thumbnails
        var options = ThumbnailOptions.Default
            .WithMaxWidth(400)
            .WithPages(new[] { 0, 1, 2 }); // Only first 3 pages

        int pageIndex = 0;
        foreach (var thumb in document.GenerateThumbnails(options))
        {
            using (thumb)
            {
                string path = Path.Combine(thumbDir, $"large_thumb_{pageIndex}.png");
                thumb.SaveAsPng(path);
                Console.WriteLine($"  Large thumbnail {pageIndex}: max 400px wide");
                pageIndex++;
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Example 13: Thumbnail quality settings
    /// </summary>
    private static async Task Example13_ThumbnailQuality(PdfDocument document, string outputDir)
    {
        Console.WriteLine("Example 13: Thumbnail Quality Comparison");

        string lowDir = Path.Combine(outputDir, "thumbs_low");
        string highDir = Path.Combine(outputDir, "thumbs_high");
        Directory.CreateDirectory(lowDir);
        Directory.CreateDirectory(highDir);

        // Low quality (fastest)
        var lowQuality = ThumbnailOptions.Default
            .WithLowQuality()
            .WithPages(new[] { 0 });

        // High quality (best looking)
        var highQuality = ThumbnailOptions.Default
            .WithHighQuality()
            .WithPages(new[] { 0 });

        foreach (var thumb in document.GenerateThumbnails(lowQuality))
        {
            using (thumb)
            {
                string path = Path.Combine(lowDir, $"thumb_low_0.png");
                thumb.SaveAsPng(path);
                Console.WriteLine($"  Low quality thumbnail generated (fastest)");
            }
        }

        foreach (var thumb in document.GenerateThumbnails(highQuality))
        {
            using (thumb)
            {
                string path = Path.Combine(highDir, $"thumb_high_0.png");
                thumb.SaveAsPng(path);
                Console.WriteLine($"  High quality thumbnail generated (best looking)");
            }
        }
        Console.WriteLine();
    }

    #endregion

    /// <summary>
    /// Show overview of all options classes
    /// </summary>
    private static void ShowOptionsOverview()
    {
        Console.WriteLine("=== Options Classes Overview ===\n");

        Console.WriteLine("1. ImageGenerationOptions - Control page rendering");
        Console.WriteLine("   - ForPages(int[] indices)     → Specific pages");
        Console.WriteLine("   - ForRange(int start, int count) → Page range");
        Console.WriteLine("   - ForAllPages()               → All pages");
        Console.WriteLine("   - WithDpi(int dpi)            → Set DPI");
        Console.WriteLine("   - WithScale(double scale)     → Set scale factor");
        Console.WriteLine("   - WithTransparency()          → Enable transparency");
        Console.WriteLine("   - WithBackgroundColor(uint)   → Set background color");
#if NET8_0_OR_GREATER
        Console.WriteLine("   - ForRange(Range, pageCount)  → Range syntax support");
#endif

        Console.WriteLine("\n2. ImageSaveOptions - Control file saving");
        Console.WriteLine("   - ForAllPages(dir)            → Save all to directory");
        Console.WriteLine("   - ForPages(dir, int[])        → Save specific pages");
        Console.WriteLine("   - ForRange(dir, start, count) → Save range");
        Console.WriteLine("   - WithOutputDirectory(dir)    → Set output dir");
        Console.WriteLine("   - WithFileNamePattern(pattern)→ Set naming pattern");
        Console.WriteLine("   - WithPathGenerator(func)     → Custom path logic");

        Console.WriteLine("\n3. ThumbnailOptions - Control thumbnail generation");
        Console.WriteLine("   - WithMaxWidth(int)           → Set max width");
        Console.WriteLine("   - WithQuality(int)           → Set quality (0-2)");
        Console.WriteLine("   - WithLowQuality()           → Fast rendering");
        Console.WriteLine("   - WithMediumQuality()        → Balanced (default)");
        Console.WriteLine("   - WithHighQuality()          → Best quality");
        Console.WriteLine("   - WithPages(int[])           → Specific pages");
        Console.WriteLine("   - ForAllPages()              → All pages");

        Console.WriteLine("\n=== Example Usage Patterns ===\n");

        Console.WriteLine("Pattern 1: Static factory methods");
        Console.WriteLine("  var options = ImageGenerationOptions.ForRange(0, 10);");

        Console.WriteLine("\nPattern 2: Fluent method chaining");
        Console.WriteLine("  var options = new ImageGenerationOptions()");
        Console.WriteLine("      .WithPages(new[] { 0, 1, 2 })");
        Console.WriteLine("      .WithDpi(300)");
        Console.WriteLine("      .WithTransparency();");

        Console.WriteLine("\nPattern 3: Property initialization");
        Console.WriteLine("  var options = new ImageGenerationOptions");
        Console.WriteLine("  {");
        Console.WriteLine("      StartIndex = 0,");
        Console.WriteLine("      Count = 10,");
        Console.WriteLine("      RenderOptions = RenderOptions.Default.WithDpi(150)");
        Console.WriteLine("  };");

        Console.WriteLine("\nPattern 4: Async image generation (.NET Standard 2.1+)");
        Console.WriteLine("  await foreach (var img in doc.GenerateImagesAsync(options))");
        Console.WriteLine("  {");
        Console.WriteLine("      using (img)");
        Console.WriteLine("          img.SaveAsPng($\"page.png\");");
        Console.WriteLine("  }");

        Console.WriteLine("\n✨ All options classes support method chaining for fluent configuration!\n");
    }
}

/* Example Output:

=== PDFiumZ Options Classes Examples ===

Document has 100 pages

Example 1: Default Options
  Generated page 0: 595x842
  Generated page 1: 595x842
  ...

Example 2: Specific Pages (0, 2, 5, 10)
  Saved: output/specific_page_595x842.png
  Saved: output/specific_page_595x842.png
  ...

*/
