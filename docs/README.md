# PDFiumZ Documentation & Examples

This document provides detailed examples and usage guides for PDFiumZ.

## Contents

- [Core Operations](#core-operations)
  - [Async Operations](#async-operations)
  - [Create PDF from Scratch](#create-pdf-from-scratch)
  - [Merge and Split PDFs](#merge-and-split-pdfs)
  - [Rotate Pages](#rotate-pages)
- [Content Extraction & Rendering](#content-extraction--rendering)
  - [Render and Extract Text](#render-and-extract-text)
  - [Save All Pages as Images](#save-all-pages-as-images)
  - [Extract Images](#extract-images)
- [Advanced Features](#advanced-features)
  - [Forms and Annotations](#forms-and-annotations)
  - [Watermarks, Headers, and Footers](#watermarks-headers-and-footers)
  - [HTML to PDF Conversion](#html-to-pdf-conversion)
- [Document Generation](#document-generation)
  - [Low-Level Content Editor](#low-level-content-editor)
  - [QuestPDF-Style Fluent API](#questpdf-style-fluent-api)
- [Integrations](#integrations)
  - [SkiaSharp Integration](#skiasharp-integration)

---

## Core Operations

### Async Operations

PDFiumZ provides asynchronous versions for most long-running operations.

```csharp
using PDFiumZ.HighLevel;

// Open document asynchronously
using var document = await PdfDocument.OpenAsync("sample.pdf");
using var page = document.GetPage(0);

// Extract and search text asynchronously
var text = await page.ExtractTextAsync();
var results = await page.SearchTextAsync("PDFiumZ");

// Render to image asynchronously
using var image = await page.RenderToImageAsync();
image.SaveAsSkiaPng("output.png");

// Add watermark asynchronously
await document.AddTextWatermarkAsync("DRAFT", WatermarkPosition.Center);
await document.SaveAsync("output.pdf");
```

### Create PDF from Scratch

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();
using var page1 = document.CreatePage(PdfPageSize.A4);
using var page2 = document.CreatePage(PdfPageSize.Letter);
using var page3 = document.CreatePage(800, 600);   // Custom size

Console.WriteLine($"Created document with {document.PageCount} pages");
document.Save("new-document.pdf");
```

### Merge and Split PDFs

```csharp
using PDFiumZ.HighLevel;

// Merge multiple files
using var merged = PdfDocument.Merge("doc1.pdf", "doc2.pdf", "doc3.pdf");
Console.WriteLine($"Merged document has {merged.PageCount} pages");
merged.Save("merged.pdf");

// Split/Extract pages
using var source = PdfDocument.Open("large.pdf");
// Extract pages 0, 1, and 2
using var first3 = source.Split(0, 1, 2);
first3.Save("first-3-pages.pdf");
```

### Rotate Pages

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");

// Rotate specific pages (0, 2, 4) by 90 degrees
document.RotatePages(PdfRotation.Rotate90, 0, 2, 4);

// Rotate all pages by 180 degrees
document.RotatePages(PdfRotation.Rotate180);

// Rotate a single page via property
using var page = document.GetPage(0);
page.Rotation = PdfRotation.Rotate270;

document.Save("rotated.pdf");
```

---

## Content Extraction & Rendering

### Render and Extract Text

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);

// Render to image with custom DPI
using var image = page.RenderToImage(RenderOptions.Default.WithDpi(150));
image.SaveAsSkiaPng("page-0.png");

// Extract plain text
var text = page.ExtractText();

// Extract text with positions and formatting
var textPage = page.GetTextPage();
var charCount = textPage.CharCount;
```

### Save All Pages as Images

For more details, see [Image Generation Guide](./IMAGE_GENERATION.md).

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("sample.pdf");

// Simplest way: auto-naming (page-0.png, page-1.png, ...)
document.SaveAsImages("output/");

// Custom file name pattern
document.SaveAsImages("output/", "document-page-{0}.png");

// High-DPI rendering (300 DPI)
var options = RenderOptions.Default.WithDpi(300);
document.SaveAsImages("highres/", options: options);
```

### Extract Images

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);

// Extract all images embedded in the page
var images = page.ExtractImages();
foreach (var img in images)
{
    // img.Image contains the PdfImage object
    // img.Bounds contains the location on page
}
```

---

## Advanced Features

### Forms and Annotations

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("form.pdf");
using var page = document.GetPage(0);

// Form Fields
var allFields = page.GetFormFields();
foreach (var field in allFields)
{
    Console.WriteLine($"Field: {field.Name}, Type: {field.Type}, Value: {field.Value}");
    if (field.Type == FormFieldType.TextField)
        field.SetValue("Updated Value");
}

// Annotations
var annotCount = page.AnnotationCount;
var allAnnots = page.GetAnnotations();

// Filter by type
var highlights = page.GetAnnotations<PdfHighlightAnnotation>();
foreach (var h in highlights)
{
    h.Color = PdfColor.Yellow;
}
```

### Watermarks, Headers, and Footers

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");

// Text Watermark
document.AddTextWatermark(
    "CONFIDENTIAL",
    WatermarkPosition.Center,
    new WatermarkOptions
    {
        Opacity = 0.3,
        Rotation = 45,
        FontSize = 48,
        Color = PdfColor.Red
    });

// Header and Footer
document.AddHeaderFooter(
    headerText: "Internal Report — Page {page} of {pages}",
    footerText: "© 2023 Company Inc.",
    options: new HeaderFooterOptions { FontSize = 10, Margin = 36 });

document.Save("protected.pdf");
```

### HTML to PDF Conversion

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();

string html = @"
    <h1 style='color: #0066CC;'>Welcome to PDFiumZ</h1>
    <p>Convert <b>HTML</b> to <i>PDF</i> easily!</p>
    <table border='1'>
        <tr><th>Item</th><th>Price</th></tr>
        <tr><td>Widget</td><td>$10</td></tr>
    </table>";

document.CreatePageFromHtml(html, new HtmlToPdfOptions { 
    Margin = new PdfMargins(36),
    PageSize = PdfPageSize.A4 
});

document.Save("html-output.pdf");
```

---

## Document Generation

### Low-Level Content Editor

Use `PdfContentEditor` for precise control over page content.

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();
using var page = document.CreatePage();
using var font = PdfFont.Load(document, PdfStandardFont.Helvetica);

using (var editor = page.BeginEdit())
{
    editor
        .WithFont(font)
        .WithFontSize(24)
        .WithTextColor(PdfColor.DarkBlue)
        .Text("Hello World", 50, 750)
        
        .WithStrokeColor(PdfColor.Red)
        .Rectangle(50, 700, 100, 50)
        
        .Commit();
}
```

### QuestPDF-Style Fluent API

For high-level declarative document generation. See [Fluent API Guide](./FLUENT_API.md).

```csharp
using PDFiumZ.Fluent;
using PDFiumZ.Fluent.Document;

using var document = new FluentDocument();
document.Content(page => {
    page.Column(col => {
        col.Item().Text("Title").FontSize(20).SemiBold();
        col.Item().PaddingVertical(10).LineHorizontal(1);
        col.Item().Text("This is a declarative document generation example.");
    });
});
document.Generate();
document.Save("fluent.pdf");
```

---

## Integrations

### SkiaSharp Integration

PDFiumZ uses SkiaSharp for rendering and image handling.

```csharp
using PDFiumZ.HighLevel;
using PDFiumZ.SkiaSharp;
using SkiaSharp;

using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);
using var image = page.RenderToImage();

// Save in different formats
image.SaveAsSkiaJpeg("output.jpg", quality: 90);
image.SaveAsSkiaWebP("output.webp");

// Use SKBitmap directly
SKBitmap bitmap = image.ToSKBitmap();
```
