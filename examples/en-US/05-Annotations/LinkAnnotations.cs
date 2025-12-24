using PDFiumZ.HighLevel;

/// <summary>
/// PDFiumZ Link Annotation Example
/// Demonstrates how to create clickable link annotations in PDF documents
/// </summary>
class LinkAnnotationsExample
{
    static void Main(string[] args)
    {
        // ============================================
        // Step 1: Initialize PDFium Library
        // ============================================
        Console.WriteLine("=== PDFiumZ Link Annotation Example ===\n");
        Console.WriteLine("Step 1: Initializing PDFium library...");
        PdfiumLibrary.Initialize();
        Console.WriteLine("✓ PDFium library initialized successfully\n");

        try
        {
            var outputPath = "link-annotations-demo.pdf";

            // ============================================
            // Step 2: Create New PDF Document
            // ============================================
            Console.WriteLine("Step 2: Creating new PDF document...");
            using var document = PdfDocument.CreateNew();
            using var page = document.CreatePage(595, 842); // A4 size
            Console.WriteLine("✓ Created an A4-sized page\n");

            // ============================================
            // Step 3: Create Website Link
            // ============================================
            Console.WriteLine("Step 3: Creating website link at the top of the page...");
            var websiteBounds = new PdfRectangle(50, 750, 300, 30);
            using var websiteLink = PdfLinkAnnotation.CreateExternal(
                page,
                websiteBounds,
                "https://github.com/casbin-net/pdfiumz"
            );
            Console.WriteLine("✓ Created link to GitHub project");
            Console.WriteLine($"  Position: X={websiteBounds.X}, Y={websiteBounds.Y}");
            Console.WriteLine($"  Size: {websiteBounds.Width}x{websiteBounds.Height}\n");

            // ============================================
            // Step 4: Create Email Link
            // ============================================
            Console.WriteLine("Step 4: Creating email link...");
            var emailBounds = new PdfRectangle(50, 700, 300, 30);
            using var emailLink = PdfLinkAnnotation.CreateExternal(
                page,
                emailBounds,
                "mailto:support@example.com"
            );
            Console.WriteLine("✓ Created email link (mailto:support@example.com)\n");

            // ============================================
            // Step 5: Create Custom Color Link
            // ============================================
            Console.WriteLine("Step 5: Creating custom color link...");
            var customColorBounds = new PdfRectangle(50, 650, 300, 30);
            uint redColor = 0xFF0000FF; // Opaque red
            using var customColorLink = PdfLinkAnnotation.CreateExternal(
                page,
                customColorBounds,
                "https://example.com/custom",
                redColor
            );
            Console.WriteLine("✓ Created link with red border");
            Console.WriteLine($"  Color: ARGB=0x{redColor:X8}\n");

            // ============================================
            // Step 6: Create Multiple Links
            // ============================================
            Console.WriteLine("Step 6: Creating additional links...");
            var link1Bounds = new PdfRectangle(50, 600, 200, 30);
            var link2Bounds = new PdfRectangle(50, 550, 200, 30);
            var link3Bounds = new PdfRectangle(50, 500, 200, 30);

            using var link1 = PdfLinkAnnotation.CreateExternal(
                page, link1Bounds, "https://www.google.com");
            using var link2 = PdfLinkAnnotation.CreateExternal(
                page, link2Bounds, "https://www.microsoft.com");
            using var link3 = PdfLinkAnnotation.CreateExternal(
                page, link3Bounds, "https://www.github.com");

            Console.WriteLine("✓ Created 3 additional links");
            Console.WriteLine("  - Google");
            Console.WriteLine("  - Microsoft");
            Console.WriteLine("  - GitHub\n");

            // ============================================
            // Step 7: Save Document
            // ============================================
            Console.WriteLine("Step 7: Saving PDF document...");
            document.Save(outputPath);
            Console.WriteLine($"✓ Document saved to: {outputPath}\n");

            // ============================================
            // Step 8: Verify Link Persistence
            // ============================================
            Console.WriteLine("Step 8: Reloading and verifying links...");
            using var loadedDoc = PdfDocument.Open(outputPath);
            using var loadedPage = loadedDoc.GetPage(0);

            var annotations = loadedPage.GetAnnotations().ToList();
            Console.WriteLine($"✓ Found {annotations.Count} annotations in loaded document");

            var linkAnnotations = annotations.Where(a => a.Type == PdfAnnotationType.Link).ToList();
            Console.WriteLine($"✓ {linkAnnotations.Count} of them are link annotations\n");

            // ============================================
            // Complete
            // ============================================
            Console.WriteLine("=== Example Complete ===");
            Console.WriteLine($"\nYou can open '{outputPath}' to view the created link annotations.");
            Console.WriteLine("When using a PDF viewer, clicking the links will open the corresponding websites or email client.");
        }
        finally
        {
            // ============================================
            // Step 9: Cleanup Resources
            // ============================================
            Console.WriteLine("\nCleaning up resources...");
            PdfiumLibrary.Shutdown();
            Console.WriteLine("✓ PDFium library shut down");
        }
    }
}
