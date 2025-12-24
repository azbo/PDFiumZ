using PDFiumZ;
using PDFiumZ.HighLevel;
using System;

namespace ImageGenerationExample
{
    class Program
    {
        static void Main(string[] args)
        {
            PdfiumLibrary.Initialize();

            try
            {
                Console.WriteLine("=== PDF Image Generation Examples ===\n");

                using var document = PdfDocument.Open("sample.pdf");
                Console.WriteLine($"Loaded PDF with {document.PageCount} pages\n");

                // ====================================
                // Example 1: Simplest way (RECOMMENDED)
                // ====================================
                Console.WriteLine("Example 1: Simplest way - SaveAsImages()");
                var filePaths = document.SaveAsImages(ImageSaveOptions.ForAllPages("output/"));
                Console.WriteLine($"   Saved {filePaths.Length} images to output/");
                Console.WriteLine($"   Files: page-0.png, page-1.png, ...\n");

                // ====================================
                // Example 2: Custom file name pattern
                // ====================================
                Console.WriteLine("Example 2: Custom file names");
                document.SaveAsImages(new ImageSaveOptions
                {
                    OutputDirectory = "output/",
                    FileNamePattern = "document-{0:D3}.png"
                });
                Console.WriteLine("   Files: document-000.png, document-001.png, ...\n");

                // ====================================
                // Example 3: Using function for full control
                // ====================================
                Console.WriteLine("Example 3: Custom path generator");
                document.SaveAsImages(ImageSaveOptions.WithPathGenerator(
                    pathGenerator: pageIndex => $"output/custom/{DateTime.Now:yyyyMMdd}-page-{pageIndex}.png"
                ));
                Console.WriteLine("   Files: 20231215-page-0.png, ...\n");

                // ====================================
                // Example 4: Save only specific pages
                // ====================================
                Console.WriteLine("Example 4: Save pages 0-2");
                document.SaveAsImages(ImageSaveOptions.ForRange("output/partial/", 0, 3));
                Console.WriteLine("   Saved first 3 pages only\n");

                // ====================================
                // Example 5: High-quality rendering
                // ====================================
                Console.WriteLine("Example 5: High-DPI rendering (300 DPI)");
                var options = ImageSaveOptions.ForAllPages("output/highres/")
                    .WithFileNamePattern("highres-{0}.png")
                    .WithDpi(300)
                    .WithTransparency();

                document.SaveAsImages(options);
                Console.WriteLine("   Saved high-resolution images\n");

                // ====================================
                // Example 6: Manual processing (if you need custom logic)
                // ====================================
                Console.WriteLine("Example 6: Manual processing with GenerateImages()");
                int index = 0;
                foreach (var image in document.GenerateImages(ImageGenerationOptions.ForAllPages()))
                {
                    using (image)
                    {
                        Console.WriteLine($"   Processing page {index}: {image.Width}x{image.Height}");
                        // Custom processing here...
                        // image.SaveAsSkiaPng($"manual-{index}.png");
                        index++;
                    }
                }
                Console.WriteLine();

                Console.WriteLine("=== All examples completed successfully! ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                PdfiumLibrary.Shutdown();
            }
        }
    }
}
