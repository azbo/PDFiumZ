# PDFiumZ Image Generation API

## Overview

PDFiumZ provides an easy-to-use image generation API that can render PDF pages as images and save them.

## API Methods

### 1. GenerateImages() - Generate Image Objects

Returns IEnumerable<PdfImage>, requiring manual handling of each image:

```csharp
using var document = PdfDocument.Open("sample.pdf");

// Generate images for all pages
foreach (var image in document.GenerateImages())
{
    using (image)
    {
        // Handle image manually...
    }
}

// Generate images for specific range
foreach (var image in document.GenerateImages(startIndex: 0, count: 5))
{
    using (image)
    {
        // Process first 5 pages...
    }
}

// Use custom render options
var options = RenderOptions.Default.WithDpi(150).WithTransparency();
foreach (var image in document.GenerateImages(options: options))
{
    using (image)
    {
        // High DPI rendering...
    }
}
```

### 2. SaveAsImages() - Direct Save to Files ⭐ Recommended

The simplest API, automatically saves all pages:

```csharp
using var document = PdfDocument.Open("sample.pdf");

// Method 1: Save to directory (Recommended - Simplest)
// Auto-generates filenames: page-0.png, page-1.png, ...
document.SaveAsImages("output/");

// Method 2: Custom filename pattern
// Generates filenames: document-page-0.png, document-page-1.png, ...
document.SaveAsImages("output/", "document-page-{0}.png");

// Method 3: Save specific range
// Save only pages 1-3
document.SaveAsImages("output/", startIndex: 0, count: 3);

// Method 4: Use custom path generator
// Fully customize filename and path
document.SaveAsImages(pageIndex => $"output/custom-{pageIndex:D3}.png");

// Method 5: Use custom path generator with range
document.SaveAsImages(pageIndex => $"output/part-{pageIndex}.png", startIndex: 10, count: 5);

// Method 6: Use high DPI options
var options = RenderOptions.Default.WithDpi(300);
document.SaveAsImages("highres/", fileNamePattern: "page-{0}.png", options: options);
```

## Complete Usage Examples

### Example 1: Simplest Usage

```csharp
PdfiumLibrary.Initialize();

try
{
    using var document = PdfDocument.Open("sample.pdf");

    // One line to save all pages as images
    var filePaths = document.SaveAsImages("output/");

    Console.WriteLine($"Saved {filePaths.Length} images:");
    foreach (var path in filePaths)
    {
        Console.WriteLine($"  {path}");
    }
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

### Example 2: Custom Filenames

```csharp
PdfiumLibrary.Initialize();

try
{
    using var document = PdfDocument.Open("invoice.pdf");

    // Use meaningful filenames
    document.SaveAsImages(
        pageIndex => $"output/invoice-{DateTime.Now:yyyyMMdd}-page{pageIndex + 1}.png");
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

### Example 3: High Quality Rendering

```csharp
PdfiumLibrary.Initialize();

try
{
    using var document = PdfDocument.Open("presentation.pdf");

    // 300 DPI high-resolution rendering with transparent background
    var options = RenderOptions.Default
        .WithDpi(300)
        .WithTransparency();

    document.SaveAsImages(
        "highres/",
        fileNamePattern: "slide-{0}.png",
        options: options);
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

### Example 4: Save Partial Pages Only

```csharp
PdfiumLibrary.Initialize();

try
{
    using var document = PdfDocument.Open("book.pdf");

    // Save only table of contents (assuming first 10 pages)
    document.SaveAsImages(
        "toc/",
        startIndex: 0,
        count: 10,
        fileNamePattern: "toc-page-{0}.png");

    // Save only chapter 1 (assuming pages 11-30)
    document.SaveAsImages(
        "chapter1/",
        startIndex: 10,
        count: 20,
        fileNamePattern: "chapter1-page-{0}.png");
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

### Example 5: Batch Process Multiple PDFs

```csharp
PdfiumLibrary.Initialize();

try
{
    var pdfFiles = Directory.GetFiles("input/", "*.pdf");

    foreach (var pdfFile in pdfFiles)
    {
        var fileName = Path.GetFileNameWithoutExtension(pdfFile);

        using var document = PdfDocument.Open(pdfFile);

        // Create separate folder for each PDF
        var outputDir = $"output/{fileName}/";
        document.SaveAsImages(outputDir);

        Console.WriteLine($"Processed: {pdfFile} -> {outputDir}");
    }
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

## API Comparison

### Old API (Complex)

```csharp
// Requires manual looping, rendering, saving
for (int i = 0; i < document.PageCount; i++)
{
    using var page = document.GetPage(i);
    using var image = page.RenderToImage();
    image.SaveAsSkiaPng($"output/page-{i}.png");
}
```

### New API (Simple) ⭐

```csharp
// One line to complete
document.SaveAsImages("output/");
```

## Parameter Descriptions

### RenderOptions

| Method | Description | Default |
|--------|-------------|---------|
| `WithDpi(int)` | Set DPI (resolution) | 96 DPI |
| `WithTransparency()` | Use transparent background | White background |
| `AddFlags(RenderFlags)` | Add render flags | - |

### Filename Pattern

- `{0}` will be replaced with page index (starting from 0)
- Example: `"page-{0}.png"` → `page-0.png`, `page-1.png`, ...
- Example: `"doc-{0:D3}.png"` → `doc-000.png`, `doc-001.png`, ... (3 digits with zero padding)

## Important Notes

1. **Auto-create directories**: `SaveAsImages` will automatically create directories that don't exist
2. **Image format**: Currently only supports PNG format (requires SkiaSharp extension)
3. **Memory management**: Images returned by `GenerateImages()` need manual disposal
4. **Performance**: For large number of pages, it's recommended to use `SaveAsImages()` rather than manual looping

## Error Handling

```csharp
try
{
    using var document = PdfDocument.Open("sample.pdf");
    document.SaveAsImages("output/");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"PDF file not found: {ex.Message}");
}
catch (PdfException ex)
{
    Console.WriteLine($"PDF error: {ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"IO error: {ex.Message}");
}
```

## Recommended Usage

✅ **Recommended**: Use `SaveAsImages()` - Simple, automatic resource management

```csharp
document.SaveAsImages("output/");
```

❌ **Not Recommended**: Manual looping - Verbose code, error-prone

```csharp
for (int i = 0; i < document.PageCount; i++)
{
    using var page = document.GetPage(i);
    using var image = page.RenderToImage();
    image.SaveAsSkiaPng($"output/page-{i}.png");
}
```
