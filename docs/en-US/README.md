# PDFiumZ Documentation & Examples 📚

PDFiumZ is a modern .NET PDF processing library with a complete high-level API and rich example code.

## 🚀 Quick Start

### Installation
```bash
dotnet add package PDFiumZ
```

### Basic Usage
```csharp
using PDFiumZ;
using PDFiumZ.HighLevel;

// Initialize library
PdfiumLibrary.Initialize();

try
{
    // Open and render PDF
    using var document = PdfDocument.Open("sample.pdf");
    using var page = document.GetPage(0);
    using var image = page.RenderToImage();

    // Save as PNG
    image.SaveAsSkiaPng("output.png");

    // Extract text
    var text = page.ExtractText();
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

## 📑 Directory Structure

### Complete Example Code
Each example is a standalone runnable project with detailed comments:

- **[01-Basics](../examples/01-Basics/)** - Getting Started
  - `GettingStarted.cs` - Quick start and basic operations demo

- **[02-Rendering](../examples/02-Rendering/)** - Rendering Features
  - `ImageGeneration.cs` - Render PDF pages as images
  - `Thumbnails.cs` - Generate page thumbnails (multiple sizes and quality levels)

- **[03-PageManipulation](../examples/03-PageManipulation/)** - Page Operations
  - `MergeSplit.cs` - Merge and split PDF documents
  - `RangeOperations.cs` - Manipulate pages using .NET 8+ Range syntax

- **[04-AdvancedOptions](../examples/04-AdvancedOptions/)** - Advanced Options
  - `OptionsConfig.cs` - Fine-grained control using option classes

### Feature Documentation
Detailed documentation organized by topic:

#### Core Features
- [Async Operations](#async-operations) - Modern async/await API
- [Create PDF](#create-pdf) - Generate documents from scratch
- [Merge & Split](#merge-split) - Document composition operations
- [Page Rotation](#page-rotation) - Adjust page orientation

#### Content Processing
- [Rendering & Text Extraction](#rendering-text-extraction) - Page rendering and content extraction
- [Image Generation](#image-generation) - Batch export pages as images
- [Thumbnail Generation](#thumbnail-generation) - Quick preview thumbnails
- [Image Extraction](#image-extraction) - Extract embedded images

#### Advanced Features
- [Form Handling](#form-handling) - Read and fill form fields
- [Annotation Features](#annotation-features) - 10+ annotation types
- [Watermarks & Headers/Footers](#watermarks-headers-footers) - Document marking and decoration
- [HTML to PDF](#html-to-pdf) - HTML content conversion
- [Security Information](#security-information) - Read encryption and permissions

#### Document Generation
- [Content Editor](#content-editor) - Low-level content control
- [Fluent API](#fluent-api) - Declarative document generation

#### Integration
- [SkiaSharp Integration](#skiasharp-integration) - Image format support
- [Range Syntax](#range-syntax) - .NET 8+ modern syntax support

## 🎯 Navigate by Need

### I want to...

#### Render PDF as Images
👉 See **[Image Generation Documentation](#image-generation)**
👉 Run **[02-Rendering/ImageGeneration.cs](../examples/02-Rendering/ImageGeneration.cs)**

#### Generate Page Thumbnails
👉 See **[Thumbnail Generation Documentation](#thumbnail-generation)**
👉 Run **[02-Rendering/Thumbnails.cs](../examples/02-Rendering/Thumbnails.cs)**

#### Merge or Split PDFs
👉 See **[Merge & Split Documentation](#merge-split)**
👉 Run **[03-PageManipulation/MergeSplit.cs](../examples/03-PageManipulation/MergeSplit.cs)**

#### Use Range Syntax (.NET 8+)
👉 See **[Range Syntax Documentation](#range-syntax)**
👉 Run **[03-PageManipulation/RangeOperations.cs](../examples/03-PageManipulation/RangeOperations.cs)**

#### Use Advanced Configuration Options
👉 See **[Options Class Reference](#options-class-reference)**
👉 Run **[04-AdvancedOptions/OptionsConfig.cs](../examples/04-AdvancedOptions/OptionsConfig.cs)**

#### Create PDF from Scratch
👉 See **[Fluent API](#fluent-api)**

#### Handle Forms and Annotations
👉 See **[Form Handling](#form-handling)** and **[Annotation Features](#annotation-features)**

## 📚 Feature Details

---

### Async Operations

PDFiumZ provides async versions for most long-running operations.

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

---

### Create PDF

Create PDF documents from scratch.

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();
using var page1 = document.CreatePage(PdfPageSize.A4);
using var page2 = document.CreatePage(PdfPageSize.Letter);
using var page3 = document.CreatePage(800, 600);   // Custom size

Console.WriteLine($"Created document with {document.PageCount} pages");
document.Save("new-document.pdf");
```

**Related Example**: [01-Basics/GettingStarted.cs](../examples/01-Basics/GettingStarted.cs)

---

### Merge & Split

Merge multiple PDFs or extract specific pages.

```csharp
using PDFiumZ.HighLevel;

// Merge multiple files
using var merged = PdfDocument.Merge("doc1.pdf", "doc2.pdf", "doc3.pdf");
Console.WriteLine($"Merged document has {merged.PageCount} pages");
merged.Save("merged.pdf");

// Split/extract pages
using var source = PdfDocument.Open("large.pdf");
// Extract pages 0, 1, and 2
using var first3 = source.Split(0, 1, 2);
first3.Save("first-3-pages.pdf");
```

**Related Example**: [03-PageManipulation/MergeSplit.cs](../examples/03-PageManipulation/MergeSplit.cs)

---

### Page Rotation

Adjust page orientation.

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");

// Rotate specific pages (0, 2, 4) by 90 degrees
document.RotatePages(PdfRotation.Rotate90, 0, 2, 4);

// Rotate all pages by 180 degrees
document.RotatePages(PdfRotation.Rotate180);

// Rotate single page via property
using var page = document.GetPage(0);
page.Rotation = PdfRotation.Rotate270;

document.Save("rotated.pdf");
```

---

### Rendering & Text Extraction

Render pages as images and extract text content.

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);

// Render to image with custom DPI
using var image = page.RenderToImage(RenderOptions.Default.WithDpi(150));
image.SaveAsSkiaPng("page-0.png");

// Extract plain text
var text = page.ExtractText();

// Extract text with position and formatting
var textPage = page.GetTextPage();
var charCount = textPage.CharCount;
```

---

### Image Generation

Batch export PDF pages as images.

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("sample.pdf");

// Simplest way: auto-naming (page-0.png, page-1.png, ...)
document.SaveAsImages("output/");

// Custom filename pattern
document.SaveAsImages("output/", "document-page-{0}.png");

// High DPI rendering (300 DPI)
var options = RenderOptions.Default.WithDpi(300);
document.SaveAsImages("highres/", options: options);
```

**Related Documentation**: [IMAGE_GENERATION.md](./IMAGE_GENERATION.md)

---

### Thumbnail Generation ✨ **New Feature**

Generate page thumbnails with multiple size and quality options.

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("sample.pdf");

// Generate thumbnail for single page
using var page = document.GetPage(0);
using var thumbnail = page.GenerateThumbnail(maxWidth: 200);
thumbnail.SaveAsSkiaPng("thumb-page-0.png");

// Generate thumbnails for all pages
var thumbnails = document.GenerateAllThumbnails(maxWidth: 150, quality: 1);
int pageNum = 0;
foreach (var thumb in thumbnails)
{
    using (thumb)
    {
        thumb.SaveAsSkiaPng($"thumbnail-{pageNum++}.png");
    }
}

// Generate thumbnails for specific pages
var selectedThumbs = document.GenerateThumbnails(
    pageIndices: new[] { 0, 5, 10 },
    maxWidth: 200,
    quality: 2  // 0=low speed/quality, 1=medium, 2=high quality
);

// Different quality levels
using var lowQuality = page.GenerateThumbnail(maxWidth: 150, quality: 0);    // Fast
using var mediumQuality = page.GenerateThumbnail(maxWidth: 150, quality: 1);  // Default
using var highQuality = page.GenerateThumbnail(maxWidth: 150, quality: 2);    // Best
```

**Related Example**: [02-Rendering/Thumbnails.cs](../examples/02-Rendering/Thumbnails.cs)

---

### Image Extraction

Extract embedded images from PDF pages.

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);

// Extract all embedded images from page
var images = page.ExtractImages();
foreach (var img in images)
{
    // img.Image contains PdfImage object
    // img.Bounds contains position information on page
}
```

---

### Form Handling

Read and fill PDF form fields.

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("form.pdf");
using var page = document.GetPage(0);

// Get all form fields
var allFields = page.GetFormFields();
foreach (var field in allFields)
{
    Console.WriteLine($"Field: {field.Name}, Type: {field.Type}, Value: {field.Value}");
    if (field.Type == FormFieldType.TextField)
        field.SetValue("Updated value");
}
```

---

### Annotation Features

Supports 10+ annotation types.

#### Reading Annotations

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("annotated.pdf");
using var page = document.GetPage(0);

// Get annotation count
var count = page.AnnotationCount;

// Get all annotations
var allAnnots = page.GetAnnotations();

// Filter by type
var highlights = page.GetAnnotations<PdfHighlightAnnotation>();
foreach (var h in highlights)
{
    Console.WriteLine($"Highlight position: {h.Bounds}");
    h.Color = PdfColor.Yellow; // Modify color

    // Get highlight regions
    var regions = h.GetQuadPoints();
}
```

#### Creating Annotations

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");
using var page = document.GetPage(0);

// Text markup annotations
var highlight = PdfHighlightAnnotation.Create(page,
    new PdfRectangle(100, 700, 200, 20),
    color: 0x80FFFF00); // Semi-transparent yellow

var underline = PdfUnderlineAnnotation.Create(page,
    new PdfRectangle(100, 650, 200, 20));

var strikeout = PdfStrikeOutAnnotation.Create(page,
    new PdfRectangle(100, 600, 200, 20));

// Shape annotations
var square = PdfSquareAnnotation.Create(page,
    new PdfRectangle(50, 500, 100, 100),
    strokeColor: PdfColor.Red,
    fillColor: PdfColor.TransparentRed);

var circle = PdfCircleAnnotation.Create(page,
    new PdfRectangle(200, 500, 100, 100),
    strokeColor: PdfColor.Blue);

// Text annotation (sticky note)
var note = PdfTextAnnotation.Create(page,
    new PdfRectangle(400, 700, 20, 20),
    "This is a sticky note");

// Free text annotation
var textBox = PdfFreeTextAnnotation.Create(page,
    new PdfRectangle(50, 300, 200, 50),
    "Editable text box");

// Ink annotation (freehand)
var ink = PdfInkAnnotation.Create(page);
ink.AddStroke(new[] {
    new PointF(100, 200), new PointF(150, 250),
    new PointF(200, 200)
});

// Stamp annotation
var stamp = PdfStampAnnotation.Create(page,
    new PdfRectangle(400, 100, 150, 50),
    PdfStampType.Approved);

// Don't forget to dispose annotations
highlight.Dispose();
// ... dispose other annotations

document.Save("annotated.pdf");
```

---

### Watermarks & Headers/Footers

Add watermarks, headers, and footers.

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");

// Text watermark
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

// Headers and footers
document.AddHeaderFooter(
    headerText: "Internal Report — Page {page} of {pages}",
    footerText: "© 2023 Company Name",
    options: new HeaderFooterOptions { FontSize = 10, Margin = 36 });

document.Save("protected.pdf");
```

---

### HTML to PDF

Convert HTML/CSS to PDF documents.

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();

string html = @"
    <h1 style='color: #0066CC;'>Welcome to PDFiumZ</h1>
    <p>Easily convert <b>HTML</b> to <i>PDF</i>!</p>
    <table border='1'>
        <tr><th>Item</th><th>Price</th></tr>
        <tr><td>Component</td><td>$10</td></tr>
    </table>";

document.CreatePageFromHtml(html, new HtmlToPdfOptions {
    Margin = new PdfMargins(36),
    PageSize = PdfPageSize.A4
});

document.Save("html-output.pdf");
```

---

### Security Information

Read PDF security settings, including encryption status and permissions.

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("protected.pdf");
var security = document.Security;

// Check encryption status
Console.WriteLine($"Encrypted: {security.IsEncrypted}");
Console.WriteLine($"User password: {security.HasUserPassword}");
Console.WriteLine($"Owner password: {security.HasOwnerPassword}");

// Check permissions (allowed operations)
Console.WriteLine($"Can print: {security.CanPrint}");
Console.WriteLine($"Can modify: {security.CanModify}");
Console.WriteLine($"Can copy: {security.CanCopy}");
Console.WriteLine($"Can annotate: {security.CanAnnotate}");
Console.WriteLine($"Can fill forms: {security.CanFillForms}");
Console.WriteLine($"Can extract content: {security.CanExtractContent}");
Console.WriteLine($"Can assemble document: {security.CanAssembleDocument}");
Console.WriteLine($"Can print high quality: {security.CanPrintHighQuality}");

// Get raw permission flags
PdfPermissions permissions = security.Permissions;
```

**Note**: PDFium only supports **reading** security information, not setting passwords or encryption. This is a PDFium limitation.

---

### Content Editor

Use `PdfContentEditor` for precise page content control.

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

---

### Fluent API

For advanced declarative document generation. See [Fluent API Guide](./FLUENT_API.md).

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

### Range Syntax (.NET 8+)

Use modern Range syntax for page operations.

```csharp
// Get first 10 pages
using var pages = document.GetPages(..10);

// Get last 5 pages
using var pages = document.GetPages(^5..);

// Get pages 5-15
using var pages = document.GetPages(5..15);

// Delete first 3 pages
document.DeletePages(..3);

// Move last 5 pages to beginning
document.MovePages(0, ^5..);
```

**Related Documentation**: [RangeSupportExamples.md](./RangeSupportExamples.md)
**Related Example**: [03-PageManipulation/RangeOperations.cs](../examples/03-PageManipulation/RangeOperations.cs)

---

### SkiaSharp Integration

PDFiumZ uses SkiaSharp for rendering and image processing.

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

---

## 🎯 Learning Path

### Beginners
1. **[01-Basics/GettingStarted.cs](../examples/01-Basics/GettingStarted.cs)** - Run basic examples
2. Read the "Quick Start" section of this document
3. Try modifying example code

### Intermediate Developers
1. **[02-Rendering/](../examples/02-Rendering/)** - Rendering examples
2. **[03-PageManipulation/](../examples/03-PageManipulation/)** - Page manipulation examples
3. Read [IMAGE_GENERATION.md](./IMAGE_GENERATION.md)

### Advanced Users
1. **[04-AdvancedOptions/](../examples/04-AdvancedOptions/)** - Advanced options examples
2. Read [FLUENT_API.md](./FLUENT_API.md)
3. Explore [RangeSupportExamples.md](./RangeSupportExamples.md)

---

## 🔗 Related Resources

- **[GitHub Repository](https://github.com/yourusername/PDFiumZ)** - Source code and issue tracking
- **[Complete Example Code](../examples/)** - All example projects
- **[API Quick Reference](./Reference/API_Quick_Reference.md)** - Common API lookup
- **[Changelog](../CHANGELOG.md)** - Version update history

---

**PDFiumZ** - Modern PDF Processing Library for .NET
