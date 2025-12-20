# PDFiumCore High-Level API Guide

## Table of Contents
- [Getting Started](#getting-started)
- [Document Management](#document-management)
- [Page Operations](#page-operations)
- [Rendering](#rendering)
- [Text Extraction](#text-extraction)
- [Bookmarks](#bookmarks)
- [Page Manipulation](#page-manipulation)
- [Saving Documents](#saving-documents)
- [Best Practices](#best-practices)

## Getting Started

### Installation

```bash
dotnet add package PDFiumCore
dotnet add package PDFiumCore.SkiaSharp  # Optional: for image export
```

### Basic Setup

```csharp
using PDFiumCore;
using PDFiumCore.HighLevel;

// Initialize library (call once at application start)
PdfiumLibrary.Initialize();

try
{
    // Your PDF operations here
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

## Document Management

### Opening Documents

```csharp
// From file path
using var document = PdfDocument.Open("sample.pdf");

// With password
using var encrypted = PdfDocument.Open("protected.pdf", "password");

// From memory (byte array)
byte[] pdfData = File.ReadAllBytes("sample.pdf");
using var document = PdfDocument.OpenFromMemory(pdfData);
```

### Document Properties

```csharp
using var document = PdfDocument.Open("sample.pdf");

Console.WriteLine($"Pages: {document.PageCount}");
Console.WriteLine($"File version: {document.FileVersion}");  // e.g., 17 for PDF 1.7
Console.WriteLine($"Permissions: {document.Permissions}");

// Check if file was loaded from disk
if (document.FilePath != null)
{
    Console.WriteLine($"Loaded from: {document.FilePath}");
}
```

## Page Operations

### Accessing Pages

```csharp
using var document = PdfDocument.Open("sample.pdf");

// Get single page (zero-based index)
using var page = document.GetPage(0);

Console.WriteLine($"Width: {page.Width} points");    // 1 point = 1/72 inch
Console.WriteLine($"Height: {page.Height} points");
Console.WriteLine($"Rotation: {page.Rotation}°");    // 0, 90, 180, or 270

// Get page size as tuple
var (width, height) = page.Size;
```

### Iterating Pages

```csharp
using var document = PdfDocument.Open("sample.pdf");

for (int i = 0; i < document.PageCount; i++)
{
    using var page = document.GetPage(i);
    Console.WriteLine($"Page {i + 1}: {page.Width} x {page.Height}");
}
```

## Rendering

### Basic Rendering

```csharp
using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);

// Render with default settings (72 DPI, white background)
using var image = page.RenderToImage();

Console.WriteLine($"Rendered: {image.Width} x {image.Height} pixels");
Console.WriteLine($"Format: {image.Format}");  // BGRA or BGRx
Console.WriteLine($"Stride: {image.Stride} bytes per row");
```

### Advanced Rendering Options

```csharp
// Fluent API for rendering configuration
var options = RenderOptions.Default
    .WithDpi(150)                                    // 150 DPI (2.08x scale)
    .WithScale(2.0)                                  // Or use custom scale
    .WithTransparency()                              // Transparent background
    .WithBackgroundColor(0xFFFFFFFF)                 // Or custom color (ARGB)
    .AddFlags(RenderFlags.OptimizeTextForLcd)       // LCD text optimization
    .AddFlags(RenderFlags.RenderAnnotations);       // Include annotations

using var image = page.RenderToImage(options);
```

### Available Render Flags

```csharp
RenderFlags.RenderAnnotations        // Render annotations
RenderFlags.OptimizeTextForLcd       // Optimize text for LCD displays
RenderFlags.NoNativeText             // Don't use native text rendering
RenderFlags.Grayscale                // Render in grayscale
RenderFlags.LimitedImageCacheSize    // Limit image cache size
RenderFlags.ForceHalftone            // Force halftone for image stretching
RenderFlags.PrintingMode             // Render for printing
RenderFlags.RenderFormFill           // Render form fields
RenderFlags.NoSmoothText             // Disable text anti-aliasing
RenderFlags.NoSmoothImage            // Disable image smoothing
RenderFlags.NoSmoothPath             // Disable path anti-aliasing
```

### Working with Image Data

```csharp
using var image = page.RenderToImage();

// Get raw pixel buffer as IntPtr
IntPtr buffer = image.Buffer;

// Get pixel data as Span<byte> (zero-copy)
Span<byte> pixels = image.GetPixelSpan();

// Copy to byte array
byte[] pixelData = image.ToByteArray();

// Calculate total buffer size
int bufferSize = image.Stride * image.Height;
```

### Saving Images (with SkiaSharp extension)

```csharp
using PDFiumCore.SkiaSharp;

using var image = page.RenderToImage();

// Save as PNG
image.SaveAsSkiaPng("output.png", quality: 100);

// Save as JPEG
image.SaveAsSkiaJpeg("output.jpg", quality: 90);

// Convert to SKBitmap for further processing
using var skBitmap = image.ToSkiaBitmap();
// ... use SkiaSharp APIs
```

## Text Extraction

### Extract Page Text

```csharp
using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);

string text = page.ExtractText();
Console.WriteLine($"Extracted {text.Length} characters");
Console.WriteLine(text);
```

### Extract Text from All Pages

```csharp
using var document = PdfDocument.Open("sample.pdf");

for (int i = 0; i < document.PageCount; i++)
{
    using var page = document.GetPage(i);
    string text = page.ExtractText();
    Console.WriteLine($"--- Page {i + 1} ---");
    Console.WriteLine(text);
}
```

## Bookmarks

### Listing Bookmarks

```csharp
using var document = PdfDocument.Open("sample.pdf");

// Get first bookmark
var firstBookmark = document.GetFirstBookmark();
if (firstBookmark != null)
{
    Console.WriteLine($"First: {firstBookmark.Title}");
}

// Enumerate all root-level bookmarks
foreach (var bookmark in document.GetBookmarks())
{
    Console.WriteLine(bookmark.Title);
}
```

### Navigating Bookmark Tree

```csharp
foreach (var bookmark in document.GetBookmarks())
{
    // Get bookmark properties
    Console.WriteLine($"Title: {bookmark.Title}");
    Console.WriteLine($"Child count: {bookmark.ChildCount}");

    // Get destination
    var dest = bookmark.Destination;
    if (dest != null && dest.HasValidPage)
    {
        Console.WriteLine($"Points to page: {dest.PageIndex}");
    }

    // Navigate to first child
    var child = bookmark.GetFirstChild();
    if (child != null)
    {
        Console.WriteLine($"  First child: {child.Title}");
    }

    // Enumerate all children
    foreach (var childBookmark in bookmark.GetChildren())
    {
        Console.WriteLine($"  - {childBookmark.Title}");
    }

    // Recursively get all descendants
    foreach (var descendant in bookmark.GetAllDescendants())
    {
        Console.WriteLine($"  Descendant: {descendant.Title}");
    }
}
```

### Finding Bookmarks

```csharp
// Search by exact title
var bookmark = document.FindBookmark("Chapter 1");
if (bookmark != null)
{
    Console.WriteLine($"Found: {bookmark.Title}");
    var dest = bookmark.Destination;
    if (dest?.HasValidPage == true)
    {
        // Jump to bookmark destination
        using var page = document.GetPage(dest.PageIndex);
        // ... render or process page
    }
}
```

## Page Manipulation

### Inserting Pages

```csharp
using var document = PdfDocument.Open("sample.pdf");

// Insert blank A4 page at beginning (595 x 842 points)
document.InsertBlankPage(0, 595, 842);

// Insert Letter size page (612 x 792 points)
document.InsertBlankPage(1, 612, 792);

// Insert at end
document.InsertBlankPage(document.PageCount, 595, 842);
```

### Deleting Pages

```csharp
// Delete specific page (zero-based index)
document.DeletePage(0);

// Delete last page
document.DeletePage(document.PageCount - 1);

// Delete multiple pages
for (int i = 2; i >= 0; i--)  // Delete in reverse to maintain indices
{
    document.DeletePage(i);
}
```

### Moving Pages

```csharp
// Move single page to end
document.MovePages(new[] { 0 }, document.PageCount - 1);

// Move multiple pages to position 2
document.MovePages(new[] { 5, 6, 7 }, 2);

// Swap pages
document.MovePages(new[] { 0 }, 2);  // Move page 0 to position 2
```

### Importing Pages

```csharp
using var source = PdfDocument.Open("source.pdf");
using var target = PdfDocument.Open("target.pdf");

// Import all pages to end
target.ImportPages(source);

// Import specific pages by range (1-based page numbers)
target.ImportPages(source, "1,3,5-7");

// Import to specific position
target.ImportPages(source, "1-3", insertAtIndex: 0);

// Import by exact indices (zero-based)
target.ImportPagesAt(source, new[] { 0, 2, 4 }, insertAtIndex: 1);
```

### Common Page Manipulation Scenarios

```csharp
// Duplicate a page
using var document = PdfDocument.Open("sample.pdf");
using var temp = PdfDocument.OpenFromMemory(File.ReadAllBytes("sample.pdf"));
document.ImportPagesAt(temp, new[] { 0 }, insertAtIndex: 1);

// Reverse page order
var pageCount = document.PageCount;
for (int i = 0; i < pageCount - 1; i++)
{
    document.MovePages(new[] { pageCount - 1 }, i);
}

// Remove every other page
for (int i = document.PageCount - 1; i >= 0; i -= 2)
{
    document.DeletePage(i);
}
```

## Saving Documents

### Basic Save

```csharp
using var document = PdfDocument.Open("sample.pdf");

// Modify document...
document.InsertBlankPage(0, 595, 842);

// Save to file
document.SaveToFile("modified.pdf");
```

### Complete Workflow Example

```csharp
using var source1 = PdfDocument.Open("part1.pdf");
using var source2 = PdfDocument.Open("part2.pdf");
using var combined = PdfDocument.Open("template.pdf");

// Insert blank cover page
combined.InsertBlankPage(0, 595, 842);

// Import pages from multiple sources
combined.ImportPages(source1);
combined.ImportPages(source2);

// Delete unwanted pages
combined.DeletePage(5);

// Save result
combined.SaveToFile("combined.pdf");
```

## Best Practices

### Resource Management

```csharp
// ✅ GOOD: Use 'using' statements
using (var document = PdfDocument.Open("sample.pdf"))
{
    using (var page = document.GetPage(0))
    {
        using (var image = page.RenderToImage())
        {
            // Use image...
        }
    }
}

// ✅ BETTER: Use 'using' declarations (C# 8.0+)
using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);
using var image = page.RenderToImage();
// Automatically disposed at end of scope

// ❌ BAD: Not disposing resources
var document = PdfDocument.Open("sample.pdf");  // Memory leak!
```

### Library Initialization

```csharp
// ✅ GOOD: Initialize once, shutdown once
public class PdfService
{
    static PdfService()
    {
        PdfiumLibrary.Initialize();
    }

    // ... PDF operations
}

// Register cleanup on application shutdown
AppDomain.CurrentDomain.ProcessExit += (s, e) => PdfiumLibrary.Shutdown();

// ❌ BAD: Initializing for every operation
void ProcessPdf(string path)
{
    PdfiumLibrary.Initialize();  // Don't do this repeatedly!
    using var doc = PdfDocument.Open(path);
    // ...
    PdfiumLibrary.Shutdown();  // Don't do this until app exit!
}
```

### Error Handling

```csharp
try
{
    using var document = PdfDocument.Open("sample.pdf", "password");
    // ... operations
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.FileName}");
}
catch (PdfPasswordException ex)
{
    Console.WriteLine("Incorrect password or password required");
}
catch (PdfLoadException ex)
{
    Console.WriteLine($"Failed to load PDF: {ex.Message}");
}
catch (PdfRenderException ex)
{
    Console.WriteLine($"Rendering failed: {ex.Message}");
}
catch (PdfException ex)
{
    Console.WriteLine($"PDF operation failed: {ex.Message}");
}
```

### Performance Tips

```csharp
// ✅ Cache RenderOptions for reuse
var highDpiOptions = RenderOptions.Default.WithDpi(150);

for (int i = 0; i < document.PageCount; i++)
{
    using var page = document.GetPage(i);
    using var image = page.RenderToImage(highDpiOptions);  // Reuse options
    // ...
}

// ✅ Use zero-copy access when possible
using var image = page.RenderToImage();
Span<byte> pixels = image.GetPixelSpan();  // No memory copy
// Process pixels directly...

// ❌ Avoid unnecessary copies
byte[] copy = image.ToByteArray();  // Creates copy - only if needed
```

### Thread Safety

```csharp
// ⚠️ Important: Document/Page objects are NOT thread-safe
// Create separate instances per thread

// ✅ GOOD: One document per thread
Parallel.For(0, fileCount, i =>
{
    using var document = PdfDocument.Open($"file{i}.pdf");
    // Process document...
});

// ❌ BAD: Sharing document across threads
using var document = PdfDocument.Open("sample.pdf");
Parallel.For(0, document.PageCount, i =>
{
    using var page = document.GetPage(i);  // Race condition!
});
```

## API Reference Summary

### PdfiumLibrary (Static)
- `Initialize()` - Initialize PDFium library
- `Shutdown()` - Shutdown PDFium library
- `IsInitialized` - Check initialization status

### PdfDocument
- **Creation**: `Open()`, `OpenFromMemory()`
- **Properties**: `PageCount`, `FileVersion`, `Permissions`, `FilePath`
- **Pages**: `GetPage()`
- **Bookmarks**: `GetFirstBookmark()`, `GetBookmarks()`, `FindBookmark()`
- **Manipulation**: `InsertBlankPage()`, `DeletePage()`, `MovePages()`, `ImportPages()`, `ImportPagesAt()`
- **Saving**: `SaveToFile()`

### PdfPage
- **Properties**: `Index`, `Width`, `Height`, `Rotation`, `Size`
- **Operations**: `RenderToImage()`, `ExtractText()`

### PdfImage
- **Properties**: `Width`, `Height`, `Stride`, `Format`, `Buffer`
- **Methods**: `GetPixelSpan()`, `ToByteArray()`
- **Extensions** (SkiaSharp): `ToSkiaBitmap()`, `SaveAsSkiaPng()`, `SaveAsSkiaJpeg()`

### RenderOptions (Immutable Record)
- **Methods**: `WithDpi()`, `WithScale()`, `WithViewport()`, `WithTransparency()`, `WithBackgroundColor()`, `AddFlags()`
- **Properties**: `Dpi`, `Scale`, `Viewport`, `HasTransparency`, `BackgroundColor`, `Flags`

### PdfBookmark
- **Properties**: `Title`, `ChildCount`, `Destination`
- **Navigation**: `GetFirstChild()`, `GetNextSibling()`, `GetChildren()`, `GetAllDescendants()`

### PdfDestination
- **Properties**: `PageIndex`, `HasValidPage`

## Exception Hierarchy

```
Exception
└── PdfException - Base exception for all PDF operations
    ├── PdfLoadException - Failed to load/open document
    ├── PdfPasswordException - Password required or incorrect
    └── PdfRenderException - Rendering operation failed
```

## Version Compatibility

- **Minimum .NET**: .NET 8.0+
- **Supported Platforms**: Windows (x64, x86), Linux (x64), macOS (x64)
- **PDFium Version**: 145.0.7578.0 (chromium/7578)

## Additional Resources

- [Official Repository](https://github.com/Dtronix/PDFiumCore)
- [NuGet Package](https://www.nuget.org/packages/PDFiumCore)
- [PDFium Documentation](https://pdfium.googlesource.com/pdfium/)
- [Report Issues](https://github.com/Dtronix/PDFiumCore/issues)
