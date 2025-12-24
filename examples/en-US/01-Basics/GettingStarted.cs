using PDFiumZ;
using PDFiumZ.HighLevel;
using System;

namespace GettingStartedExample;

/// <summary>
/// PDFiumZ Quick Start Example
/// Demonstrates basic operations of PDFiumZ
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        // ============================================
        // Step 1: Initialize PDFium library
        // ============================================
        Console.WriteLine("=== PDFiumZ Quick Start Example ===\n");
        Console.WriteLine("Step 1: Initializing PDFium library...");
        PdfiumLibrary.Initialize();
        Console.WriteLine("✓ PDFium library initialized successfully\n");

        try
        {
            // ============================================
            // Step 2: Create a simple PDF document
            // ============================================
            Console.WriteLine("Step 2: Creating new document...");
            using var document = PdfDocument.CreateNew();

            // Add A4 page
            using var page = document.CreatePage(PdfPageSize.A4);
            Console.WriteLine($"✓ Created {document.PageCount} A4 page(s)");

            // Add text to the page
            using var font = PdfFont.Load(document, PdfStandardFont.HelveticaBold);
            using var editor = page.BeginEdit();

            editor
                .WithFont(font)
                .WithFontSize(24)
                .WithTextColor(PdfColor.DarkBlue)
                .Text("Welcome to PDFiumZ", 100, 700)

                .WithFontSize(14)
                .WithTextColor(PdfColor.Black)
                .Text("This is your first PDF document", 100, 650)
                .Text($"Created at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", 100, 630)

                // Draw rectangle
                .WithStrokeColor(PdfColor.Red)
                .WithLineWidth(2)
                .Rectangle(new PdfRectangle(100, 550, 400, 50))

                // Draw circle
                .WithFillColor(PdfColor.LightBlue)
                .Circle(300, 450, 30)
                .Commit();

            Console.WriteLine("✓ Text and graphics added\n");

            // ============================================
            // Step 3: Save document
            // ============================================
            Console.WriteLine("Step 3: Saving document...");
            string outputPath = "getting-started-output.pdf";
            document.Save(outputPath);
            Console.WriteLine($"✓ Document saved to: {outputPath}\n");

            // ============================================
            // Step 4: Read document information
            // ============================================
            Console.WriteLine("Step 4: Reading document information...");

            // Reopen saved document
            using var openedDoc = PdfDocument.Open(outputPath);
            Console.WriteLine($"  Page count: {openedDoc.PageCount}");

            // Read first page
            using var firstPage = openedDoc.GetPage(0);
            Console.WriteLine($"  Page 1 size: {firstPage.Width:F2} x {firstPage.Height:F2} points");
            Console.WriteLine($"  Page 1 rotation: {firstPage.Rotation}");

            // Extract text
            var text = firstPage.ExtractText();
            Console.WriteLine($"  Extracted text length: {text.Length} characters");

            // Read metadata (if available)
            var metadata = openedDoc.Metadata;
            if (metadata != null)
            {
                // Metadata available
                Console.WriteLine($"  Metadata: loaded");
            }

            Console.WriteLine("✓ Document information read complete\n");

            // ============================================
            // Step 5: Render page to image
            // ============================================
            Console.WriteLine("Step 5: Rendering page to image...");

            using var image = firstPage.RenderToImage(RenderOptions.Default.WithDpi(150));
            string imagePath = "getting-started-output.png";
            image.SaveAsPng(imagePath);
            Console.WriteLine($"✓ Page rendered to image: {imagePath}");
            Console.WriteLine($"  Image size: {image.Width} x {image.Height} pixels\n");

            // ============================================
            // Step 6: Page operations
            // ============================================
            Console.WriteLine("Step 6: Page operation examples...");

            // Add second page
            using var page2 = document.CreatePage(PdfPageSize.A4);
            using var editor2 = page2.BeginEdit();
            editor2
                .WithFont(font)
                .WithFontSize(18)
                .WithTextColor(PdfColor.Black)
                .Text("This is page 2", 100, 700)
                .Commit();

            Console.WriteLine($"✓ Document now has {document.PageCount} pages");

            // Rotate second page
            page2.Rotation = PdfRotation.Rotate90;
            Console.WriteLine($"✓ Page 2 rotated 90 degrees\n");

            // Save modified document
            string modifiedPath = "getting-started-modified.pdf";
            document.Save(modifiedPath);
            Console.WriteLine($"✓ Modified document saved: {modifiedPath}\n");

            // ============================================
            // Summary
            // ============================================
            Console.WriteLine("=== Example Complete ===");
            Console.WriteLine("\nYou have learned:");
            Console.WriteLine("  ✓ How to initialize and cleanup PDFium library");
            Console.WriteLine("  ✓ How to create new PDF documents");
            Console.WriteLine("  ✓ How to add pages and content");
            Console.WriteLine("  ✓ How to save and open documents");
            Console.WriteLine("  ✓ How to read document information");
            Console.WriteLine("  ✓ How to render pages to images");
            Console.WriteLine("  ✓ How to manipulate pages (add, rotate)");
            Console.WriteLine("\nGenerated files:");
            Console.WriteLine($"  - {outputPath}");
            Console.WriteLine($"  - {imagePath}");
            Console.WriteLine($"  - {modifiedPath}");
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
            // ============================================
            // Cleanup resources
            // ============================================
            Console.WriteLine("\nStep 7: Cleaning up resources...");
            PdfiumLibrary.Shutdown();
            Console.WriteLine("✓ PDFium library shutdown");
        }
    }
}
