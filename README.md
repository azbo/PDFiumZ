# <img src="./src/PDFiumZ/icon.png" width="48"> PDFiumZ [![NuGet](https://img.shields.io/nuget/v/PDFiumZ.svg?maxAge=60)](https://www.nuget.org/packages/PDFiumZ)

PDFiumZ is a modern .NET 10.0+ wrapper for [PDFium](https://pdfium.googlesource.com/pdfium/) with a comprehensive high-level API. Built on [PDFium binaries](https://github.com/bblanchon/pdfium-binaries) with P/Invoke bindings. Supports Linux-x64, OSX-x64, Win-x64, Win-x86.

**Current PDFium Version: 145.0.7578.0** (chromium/7578) - Includes improved text reading capabilities

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package PDFiumZ
```

Or via Package Manager Console:

```powershell
Install-Package PDFiumZ
```

## Quick Start

```csharp
using PDFiumZ;
using PDFiumZ.HighLevel;

// Initialize library (call once at application start)
PdfiumLibrary.Initialize();

try
{
    // Open and render a PDF
    using var document = PdfDocument.Open("sample.pdf");
    using var page = document.GetPage(0);
    using var image = page.RenderToImage();

    // Save as PNG
    image.SaveAsSkiaPng("output.png");

    // Extract text
    var text = page.ExtractText();
    Console.WriteLine(text);
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

### Create PDF from Scratch

```csharp
// Create a new empty PDF document
using var document = PdfDocument.CreateNew();

// Add pages with different sizes
using var page1 = document.CreatePage(595, 842);   // A4 (210mm x 297mm)
using var page2 = document.CreatePage(612, 792);   // Letter (8.5" x 11")
using var page3 = document.CreatePage(800, 600);   // Custom size

Console.WriteLine($"Created document with {document.PageCount} pages");

// Save the new document
document.SaveToFile("new-document.pdf");
```

### Merge and Split PDFs

```csharp
// Merge multiple PDF documents
using var merged = PdfDocument.Merge("doc1.pdf", "doc2.pdf", "doc3.pdf");
Console.WriteLine($"Merged document has {merged.PageCount} pages");
merged.SaveToFile("merged.pdf");

// Split PDF by extracting page range
using var source = PdfDocument.Open("large.pdf");
using var split = source.Split(startIndex: 0, pageCount: 10);  // Extract first 10 pages
split.SaveToFile("first-10-pages.pdf");
```

### Add Watermarks

```csharp
using var document = PdfDocument.Open("document.pdf");

// Add watermark to all pages
document.AddTextWatermark(
    "CONFIDENTIAL",
    WatermarkPosition.Center,
    new WatermarkOptions
    {
        Opacity = 0.3,        // 30% opacity
        Rotation = 45,        // 45 degrees
        FontSize = 48,        // 48 points
        Color = 0xFF0000FF    // Blue (ARGB format)
    });

document.SaveToFile("watermarked.pdf");
```

### Rotate Pages

```csharp
using var document = PdfDocument.Open("document.pdf");

// Rotate individual pages
document.RotatePages(PdfRotation.Rotate90, 0, 2, 4);  // Rotate pages 0, 2, 4 by 90°

// Rotate all pages
document.RotateAllPages(PdfRotation.Rotate180);

// Or rotate a single page
using var page = document.GetPage(0);
page.Rotation = PdfRotation.Rotate270;

document.SaveToFile("rotated.pdf");
```

## Features

### High-Level API (Recommended)

PDFiumZ provides a modern, fluent API that makes PDF operations simple and intuitive:

```csharp
using PDFiumZ;
using PDFiumZ.HighLevel;

// Initialize library (call once at application start)
PdfiumLibrary.Initialize();

try
{
    // Open document with automatic resource management
    using var document = PdfDocument.Open("sample.pdf");
    Console.WriteLine($"Pages: {document.PageCount}");

    // Render a page to image
    using var page = document.GetPage(0);
    using var image = page.RenderToImage();

    // Save as PNG (requires PDFiumZ.SkiaSharp extension package)
    image.SaveAsSkiaPng("output.png");

    // Extract text
    var text = page.ExtractText();
    Console.WriteLine(text);

    // Page manipulation
    document.InsertBlankPage(0, 595, 842);  // Insert A4 page at beginning
    document.DeletePage(2);                  // Delete page 2
    document.MovePages(new[] { 0, 1 }, 3);  // Move pages to new position

    // Import pages from another document
    using var source = PdfDocument.Open("other.pdf");
    document.ImportPages(source, "1,3,5-7");  // Import specific pages

    // Save modified document
    document.SaveToFile("modified.pdf");

    // Navigate bookmarks
    foreach (var bookmark in document.GetBookmarks())
    {
        Console.WriteLine($"{bookmark.Title} -> Page {bookmark.Destination?.PageIndex}");

        // Recursively iterate children
        foreach (var child in bookmark.GetAllDescendants())
        {
            Console.WriteLine($"  {child.Title}");
        }
    }

    // Access document metadata
    var meta = document.Metadata;
    Console.WriteLine($"Title: {meta.Title}");
    Console.WriteLine($"Author: {meta.Author}");
    Console.WriteLine($"Producer: {meta.Producer}");
    Console.WriteLine($"Creation Date: {meta.CreationDate}");

    // Read form fields
    using var formPage = document.GetPage(0);
    foreach (var field in formPage.GetFormFields())
    {
        using (field)
        {
            Console.WriteLine($"Field: {field.Name}");
            Console.WriteLine($"Type: {field.FieldType}");
            Console.WriteLine($"Value: {field.Value}");

            if (field.FieldType == PdfFormFieldType.CheckBox)
                Console.WriteLine($"Checked: {field.IsChecked}");

            if (field.FieldType == PdfFormFieldType.ComboBox)
            {
                var options = field.GetAllOptions();
                Console.WriteLine($"Options: {string.Join(", ", options)}");
            }
        }
    }

    // Work with annotations
    using var annotPage = document.GetPage(0);

    // Create a highlight annotation (text markup)
    var highlightBounds = new PdfRectangle(100, 700, 200, 20);
    var highlight = PdfHighlightAnnotation.Create(annotPage, highlightBounds, 0x80FFFF00); // Yellow, 50% opacity
    highlight.SetQuadPoints(new[]
    {
        new PdfRectangle(100, 700, 200, 10),
        new PdfRectangle(100, 710, 150, 10)
    });

    // Create an underline annotation
    var underline = PdfUnderlineAnnotation.Create(
        annotPage,
        new PdfRectangle(100, 650, 200, 15),
        0xFFFF0000); // Red underline

    // Create a strikeout annotation
    var strikeout = PdfStrikeOutAnnotation.Create(
        annotPage,
        new PdfRectangle(100, 600, 200, 15),
        0xFF0000FF); // Blue strikeout

    // Create a line annotation
    var line = PdfLineAnnotation.Create(
        annotPage,
        startX: 50, startY: 550,
        endX: 250, endY: 550,
        color: 0xFFFF0000,  // Red
        width: 2.0);

    // Create a rectangle annotation
    var rectangle = PdfSquareAnnotation.Create(
        annotPage,
        new PdfRectangle(300, 700, 150, 100),
        borderColor: 0xFFFF0000,  // Red border
        fillColor: 0x400000FF,    // Blue fill, 25% opacity
        borderWidth: 2.0);

    // Create a circle annotation
    var circle = PdfCircleAnnotation.Create(
        annotPage,
        new PdfRectangle(300, 550, 100, 100),
        borderColor: 0xFF00FF00,  // Green border
        fillColor: 0x4000FF00,    // Green fill, 25% opacity
        borderWidth: 2.0);

    // Create a text annotation (sticky note)
    var textAnnot = PdfTextAnnotation.Create(annotPage, 50, 450, "Important: Review this section");
    textAnnot.Author = "Reviewer";

    // Create stamp annotations
    var draftStamp = PdfStampAnnotation.Create(
        annotPage,
        new PdfRectangle(350, 400, 120, 50),
        PdfStampType.Draft);

    var approvedStamp = PdfStampAnnotation.Create(
        annotPage,
        new PdfRectangle(350, 340, 120, 50),
        PdfStampType.Approved);

    // Save document with annotations
    document.SaveToFile("annotated.pdf");

    // Clean up annotations
    highlight.Dispose();
    underline.Dispose();
    strikeout.Dispose();
    line.Dispose();
    rectangle.Dispose();
    circle.Dispose();
    textAnnot.Dispose();
    draftStamp.Dispose();
    approvedStamp.Dispose();

    // Read annotations back
    var annotCount = annotPage.GetAnnotationCount();
    Console.WriteLine($"Total annotations: {annotCount}");

    foreach (var annotation in annotPage.GetAnnotations())
    {
        using (annotation)
        {
            Console.WriteLine($"Type: {annotation.Type}, Bounds: {annotation.Bounds}");

            if (annotation is PdfHighlightAnnotation h)
            {
                var quads = h.GetQuadPoints();
                Console.WriteLine($"  Quad points: {quads.Length}");
            }
            else if (annotation is PdfTextAnnotation t)
            {
                Console.WriteLine($"  Contents: {t.Contents}");
                Console.WriteLine($"  Author: {t.Author}");
            }
            else if (annotation is PdfStampAnnotation s)
            {
                Console.WriteLine($"  Stamp type: {s.StampType}");
            }
        }
    }

    // Filter annotations by type
    var highlights = annotPage.GetAnnotations<PdfHighlightAnnotation>();
    var textAnnotations = annotPage.GetAnnotations<PdfTextAnnotation>();
    var stamps = annotPage.GetAnnotations<PdfStampAnnotation>();

    // Remove an annotation
    if (annotCount > 0)
    {
        annotPage.RemoveAnnotation(0);
        document.SaveToFile("annotations-removed.pdf");
    }

    // Content creation - add text and shapes
    using var contentPage = document.GetPage(0);

    // Load fonts
    var helvetica = PdfFont.LoadStandardFont(document, PdfStandardFont.Helvetica);
    var timesBold = PdfFont.LoadStandardFont(document, PdfStandardFont.TimesBold);

    // Begin editing page content
    using (var editor = contentPage.BeginEdit())
    {
        // Add title text
        editor.AddText("Created with PDFiumZ", 50, 750, timesBold, 24);

        // Add body text
        editor.AddText("This content was added programmatically.", 50, 700, helvetica, 14);

        // Add colored rectangles
        editor.AddRectangle(
            new PdfRectangle(50, 600, 150, 100),
            0xFFFF0000,  // Red stroke
            0x800000FF   // Blue fill, 50% opacity
        );

        // Generate content to persist changes
        editor.GenerateContent();
    }

    helvetica.Dispose();
    timesBold.Dispose();

    document.SaveToFile("output-with-content.pdf");

    // Async operations - non-blocking operations with cancellation support
    Console.WriteLine("Async Operations Demo:");

    // Async document loading
    using var asyncDoc = await PdfDocument.OpenAsync("sample.pdf");
    Console.WriteLine($"Loaded {asyncDoc.PageCount} pages asynchronously");

    // Async page rendering
    using var asyncPage = asyncDoc.GetPage(0);
    using var asyncImage = await asyncPage.RenderToImageAsync();
    Console.WriteLine($"Rendered {asyncImage.Width}x{asyncImage.Height} asynchronously");

    // Async saving
    await asyncDoc.SaveToFileAsync("output-async.pdf");

    // Cancellation support
    using var cts = new System.Threading.CancellationTokenSource();
    cts.CancelAfter(100);
    try
    {
        using var cancelDoc = await PdfDocument.OpenAsync("sample.pdf", cancellationToken: cts.Token);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Operation was canceled");
    }

    // Batch operations - efficient multi-page processing
    Console.WriteLine("Batch Operations Demo:");

    using var batchDoc = PdfDocument.Open("sample.pdf");

    // Get multiple consecutive pages
    var pages = batchDoc.GetPages(0, 3).ToList();
    Console.WriteLine($"Retrieved {pages.Count} pages in batch");
    pages.ForEach(p => p.Dispose());

    // Delete multiple pages by indices (non-consecutive)
    batchDoc.DeletePages(1, 3, 5);  // Deletes pages at indices 1, 3, and 5
    Console.WriteLine($"Pages after batch deletion: {batchDoc.PageCount}");

    // Delete consecutive page range
    batchDoc.DeletePages(0, 2);  // Deletes first 2 pages (indices 0 and 1)
    Console.WriteLine($"Pages after range deletion: {batchDoc.PageCount}");

    batchDoc.SaveToFile("output-batch.pdf");
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

**Features:**
- ✅ Modern C# API with `IDisposable` pattern
- ✅ Fluent rendering options (`WithDpi()`, `WithScale()`, `WithTransparency()`)
- ✅ **PDF creation from scratch** (create new documents and blank pages)
- ✅ **PDF merge and split** (combine multiple PDFs or extract page ranges)
- ✅ Page manipulation (insert, delete, move, import)
- ✅ **Page rotation** (rotate individual pages, batch rotation, or all pages)
- ✅ **Text watermarks** (add watermarks with custom rotation, opacity, and styling)
- ✅ Bookmark navigation with tree traversal
- ✅ Form field reading and writing (all standard field types)
- ✅ Document metadata access (title, author, dates, etc.)
- ✅ Page labels (custom page numbering)
- ✅ Hyperlink support (detect links at coordinates)
- ✅ Text extraction
- ✅ Text search with position information (case-sensitive, whole-word options)
- ✅ Image extraction from PDF pages
- ✅ Page object enumeration and type classification
- ✅ Document saving
- ✅ Zero-copy image access via `Span<byte>`
- ✅ **Annotation support** (highlight, underline, strikeout, text notes, stamps, lines, rectangles, circles)
- ✅ **Content creation** (add text, images, shapes to pages)
- ✅ **Font management** (standard PDF fonts and TrueType fonts)
- ✅ **Async API** (non-blocking operations with cancellation support)
- ✅ **Batch operations** (efficient multi-page processing)

#### Low-Level API

The low-level P/Invoke API is still available for advanced scenarios through the `fpdfview`, `fpdf_edit`, `fpdf_doc` classes.

## Build Requirements
- .NET 10.0 SDK or later

## Building from Source

1. Clone the repository
2. Run `dotnet build` in the `src` directory
3. Run `dotnet pack` to create NuGet packages

### Resources

- [PDFium Source](https://pdfium.googlesource.com/pdfium/)
- [PDFium Binaries](https://github.com/bblanchon/pdfium-binaries)
- [CppSharp](https://github.com/mono/CppSharp)

### License
This project is released under [Apache-2.0 License](LICENSE), matching the PDFium project license.

## Support the Project

If you find PDFiumZ useful, you can support the project development:

### WeChat Pay / Alipay
<div align="center">
  <img src="./docs/wechat-pay-qr.png" width="200" alt="WeChat Pay">
  <img src="./docs/alipay-qr.png" width="200" alt="Alipay">
</div>

Thank you for your support! ❤️

