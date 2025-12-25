using PDFiumZ.HighLevel;

/// <summary>
/// PDFiumZ Text Markup Annotations Example
/// Demonstrates how to create highlight, underline, and strikeout annotations in PDF documents
/// </summary>
class TextMarkupAnnotationsExample
{
    static void Main(string[] args)
    {
        // ============================================
        // Step 1: Initialize PDFium Library
        // ============================================
        Console.WriteLine("=== PDFiumZ Text Markup Annotations Example ===\n");
        Console.WriteLine("Step 1: Initializing PDFium library...");
        PdfiumLibrary.Initialize();
        Console.WriteLine("✓ PDFium library initialized successfully\n");

        try
        {
            var outputPath = "text-markup-annotations-demo.pdf";

            // ============================================
            // Step 2: Create New PDF Document
            // ============================================
            Console.WriteLine("Step 2: Creating new PDF document...");
            using var document = PdfDocument.CreateNew();
            using var page = document.CreatePage(595, 842); // A4 size
            Console.WriteLine("✓ Created an A4-sized page\n");

            // ============================================
            // Step 3: Create Highlight Annotation
            // ============================================
            Console.WriteLine("Step 3: Creating highlight annotation...");
            var highlightBounds = new PdfRectangle(50, 750, 300, 30);
            using var highlight = PdfHighlightAnnotation.Create(
                page,
                highlightBounds
                // Default color: 0x80FFFF00 (yellow with 50% opacity)
            );
            Console.WriteLine("✓ Created yellow highlight annotation");
            Console.WriteLine($"  Position: X={highlightBounds.X}, Y={highlightBounds.Y}");
            Console.WriteLine($"  Size: {highlightBounds.Width}x{highlightBounds.Height}");
            Console.WriteLine($"  Color: ARGB=0x{highlight.Color:X8}\n");

            // ============================================
            // Step 4: Create Custom Color Highlight
            // ============================================
            Console.WriteLine("Step 4: Creating custom color highlight...");
            var customHighlightBounds = new PdfRectangle(50, 700, 300, 30);
            uint greenHighlight = 0x8000FF00; // Green with 50% opacity
            using var customHighlight = PdfHighlightAnnotation.Create(
                page,
                customHighlightBounds,
                greenHighlight
            );
            Console.WriteLine("✓ Created green highlight annotation");
            Console.WriteLine($"  Color: ARGB=0x{greenHighlight:X8}\n");

            // ============================================
            // Step 5: Create Underline Annotation
            // ============================================
            Console.WriteLine("Step 5: Creating underline annotation...");
            var underlineBounds = new PdfRectangle(50, 650, 300, 30);
            using var underline = PdfUnderlineAnnotation.Create(
                page,
                underlineBounds
                // Default color: 0xFFFF0000 (red with full opacity)
            );
            Console.WriteLine("✓ Created red underline annotation");
            Console.WriteLine($"  Position: X={underlineBounds.X}, Y={underlineBounds.Y}");
            Console.WriteLine($"  Color: ARGB=0x{underline.Color:X8}\n");

            // ============================================
            // Step 6: Create Custom Color Underline
            // ============================================
            Console.WriteLine("Step 6: Creating custom color underline...");
            var customUnderlineBounds = new PdfRectangle(50, 600, 300, 30);
            uint blueUnderline = 0xFF0000FF; // Blue with full opacity
            using var customUnderline = PdfUnderlineAnnotation.Create(
                page,
                customUnderlineBounds,
                blueUnderline
            );
            Console.WriteLine("✓ Created blue underline annotation");
            Console.WriteLine($"  Color: ARGB=0x{blueUnderline:X8}\n");

            // ============================================
            // Step 7: Create StrikeOut Annotation
            // ============================================
            Console.WriteLine("Step 7: Creating strikeout annotation...");
            var strikeoutBounds = new PdfRectangle(50, 550, 300, 30);
            using var strikeout = PdfStrikeOutAnnotation.Create(
                page,
                strikeoutBounds
                // Default color: 0xFFFF0000 (red with full opacity)
            );
            Console.WriteLine("✓ Created red strikeout annotation");
            Console.WriteLine($"  Position: X={strikeoutBounds.X}, Y={strikeoutBounds.Y}");
            Console.WriteLine($"  Color: ARGB=0x{strikeout.Color:X8}\n");

            // ============================================
            // Step 8: Create Custom Color StrikeOut
            // ============================================
            Console.WriteLine("Step 8: Creating custom color strikeout...");
            var customStrikeoutBounds = new PdfRectangle(50, 500, 300, 30);
            uint blackStrikeout = 0xFF000000; // Black with full opacity
            using var customStrikeout = PdfStrikeOutAnnotation.Create(
                page,
                customStrikeoutBounds,
                blackStrikeout
            );
            Console.WriteLine("✓ Created black strikeout annotation");
            Console.WriteLine($"  Color: ARGB=0x{blackStrikeout:X8}\n");

            // ============================================
            // Step 9: Create Multiple Annotations
            // ============================================
            Console.WriteLine("Step 9: Creating multiple mixed annotations...");

            using var highlight1 = PdfHighlightAnnotation.Create(
                page, new PdfRectangle(50, 450, 200, 30), 0x80FFFF00);
            using var underline1 = PdfUnderlineAnnotation.Create(
                page, new PdfRectangle(50, 400, 200, 30), 0xFFFF0000);
            using var strikeout1 = PdfStrikeOutAnnotation.Create(
                page, new PdfRectangle(50, 350, 200, 30), 0xFF000000);

            Console.WriteLine("✓ Created 3 additional annotations\n");

            // ============================================
            // Step 10: Save Document
            // ============================================
            Console.WriteLine("Step 10: Saving PDF document...");
            document.Save(outputPath);
            Console.WriteLine($"✓ Document saved to: {outputPath}\n");

            // ============================================
            // Step 11: Verify Annotation Persistence
            // ============================================
            Console.WriteLine("Step 11: Reloading and verifying annotations...");
            using var loadedDoc = PdfDocument.Open(outputPath);
            using var loadedPage = loadedDoc.GetPage(0);

            var annotations = loadedPage.GetAnnotations().ToList();
            Console.WriteLine($"✓ Found {annotations.Count} annotations in loaded document");

            var highlightCount = annotations.Count(a => a.Type == PdfAnnotationType.Highlight);
            var underlineCount = annotations.Count(a => a.Type == PdfAnnotationType.Underline);
            var strikeoutCount = annotations.Count(a => a.Type == PdfAnnotationType.StrikeOut);

            Console.WriteLine($"  - Highlight: {highlightCount}");
            Console.WriteLine($"  - Underline: {underlineCount}");
            Console.WriteLine($"  - StrikeOut: {strikeoutCount}\n");

            // ============================================
            // Complete
            // ============================================
            Console.WriteLine("=== Example Complete ===");
            Console.WriteLine($"\nYou can open '{outputPath}' to view the created text markup annotations.");
            Console.WriteLine("When using a PDF viewer, these annotations will highlight, underline, or strikeout the specified regions.");
        }
        finally
        {
            // ============================================
            // Step 12: Cleanup Resources
            // ============================================
            Console.WriteLine("\nCleaning up resources...");
            PdfiumLibrary.Shutdown();
            Console.WriteLine("✓ PDFium library shut down");
        }
    }
}
