# PDFiumZ Documentation

## Contents

- [Examples](#examples)
  - [Create PDF from Scratch](#create-pdf-from-scratch)
  - [Merge and Split PDFs](#merge-and-split-pdfs)
  - [Add Watermarks](#add-watermarks)
  - [Rotate Pages](#rotate-pages)
  - [Check PDF Security and Permissions](#check-pdf-security-and-permissions)
  - [Annotations](#annotations)
  - [Enhanced Fluent API for Content Creation](#enhanced-fluent-api-for-content-creation)
  - [HTML to PDF Conversion](#html-to-pdf-conversion)
- [Features](#features)
- [Building](#building)
- [Benchmarks](#benchmarks)
- [Support](#support)

## Examples

### Create PDF from Scratch

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();

using var page1 = document.CreatePage(PdfPageSize.A4);
using var page2 = document.CreatePage(PdfPageSize.Letter);
using var page3 = document.CreatePage(800, 600);   // Custom size

Console.WriteLine($"Created document with {document.PageCount} pages");
document.SaveToFile("new-document.pdf");
```

### Merge and Split PDFs

```csharp
using PDFiumZ.HighLevel;

using var merged = PdfDocument.Merge("doc1.pdf", "doc2.pdf", "doc3.pdf");
Console.WriteLine($"Merged document has {merged.PageCount} pages");
merged.SaveToFile("merged.pdf");

using var source = PdfDocument.Open("large.pdf");
using var split = source.Split(startIndex: 0, pageCount: 10);
split.SaveToFile("first-10-pages.pdf");
```

### Add Watermarks

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");

document.AddTextWatermark(
    "CONFIDENTIAL",
    WatermarkPosition.Center,
    new WatermarkOptions
    {
        Opacity = 0.3,
        Rotation = 45,
        FontSize = 48,
        Color = 0xFF0000FF
    });

document.SaveToFile("watermarked.pdf");
```

### Rotate Pages

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");

document.RotatePages(PdfRotation.Rotate90, 0, 2, 4);
document.RotateAllPages(PdfRotation.Rotate180);

using var page = document.GetPage(0);
page.Rotation = PdfRotation.Rotate270;

document.SaveToFile("rotated.pdf");
```

### Check PDF Security and Permissions

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");
var security = document.Security;

if (security.IsEncrypted)
{
    Console.WriteLine($"Encryption: {security.EncryptionMethod}");
    Console.WriteLine($"Security Handler: Revision {security.SecurityHandlerRevision}");
}
else
{
    Console.WriteLine("Document is not encrypted");
}

Console.WriteLine($"Can print: {security.CanPrint}");
Console.WriteLine($"Can modify: {security.CanModifyContents}");
Console.WriteLine($"Can copy: {security.CanCopyContent}");
Console.WriteLine($"Can fill forms: {security.CanFillForms}");

var permissions = document.Permissions;
Console.WriteLine($"Permission flags: 0x{permissions:X8}");
```

PDFiumZ can read PDF security information but does not support encrypting or password-protecting PDFs. For encryption, use external tools.

### Annotations

```csharp
using PDFiumZ.HighLevel;
using System.Collections.Generic;

using var document = PdfDocument.CreateNew();
using var page = document.CreatePage(595, 842);

var highlight = PdfHighlightAnnotation.Create(page, new[] { new PdfRectangle(100, 700, 200, 20) }, 0xFFFFFF00);
var underline = PdfUnderlineAnnotation.Create(page, new[] { new PdfRectangle(100, 650, 200, 20) });
var strikeOut = PdfStrikeOutAnnotation.Create(page, new[] { new PdfRectangle(100, 600, 200, 20) });

var square = PdfSquareAnnotation.Create(page, new PdfRectangle(100, 500, 100, 50), 0xFFFF0000, 0x400000FF, 2.0);
var circle = PdfCircleAnnotation.Create(page, new PdfRectangle(250, 500, 50, 50), 0xFF00FF00);

var freeText = PdfFreeTextAnnotation.Create(page, new PdfRectangle(100, 400, 200, 50), "Hello PDFiumZ!", 0xFF000000, 12);

var paths = new List<List<(double X, double Y)>>
{
    new() { (100, 300), (150, 350), (200, 300) }
};
var ink = PdfInkAnnotation.Create(page, paths, 0xFF0000FF, 2.0);

using var image = PdfBitmap.Load("stamp.png");
var stamp = PdfStampAnnotation.Create(page, new PdfRectangle(300, 100, 100, 100), image);
```

### Enhanced Fluent API for Content Creation

PDFiumZ provides a powerful fluent API for creating PDF content with predefined colors, font sizes, and shapes.

#### Predefined Colors

Use `PdfColor` for easy color management with 40+ predefined colors:

```csharp
using PDFiumZ.HighLevel;

// Basic colors
PdfColor.Black, PdfColor.White, PdfColor.Red, PdfColor.Green, PdfColor.Blue

// Extended colors
PdfColor.Orange, PdfColor.Purple, PdfColor.Pink, PdfColor.Brown, PdfColor.Gold

// Shades
PdfColor.DarkRed, PdfColor.LightRed, PdfColor.DarkBlue, PdfColor.LightBlue

// Highlights (with transparency)
PdfColor.HighlightYellow, PdfColor.HighlightGreen, PdfColor.HighlightBlue

// Create custom colors
var customColor = PdfColor.FromHex("#FF6B6B");
var semiTransparent = PdfColor.FromRgb(255, 0, 0, opacity: 0.5);
var adjusted = PdfColor.WithOpacity(PdfColor.Red, 0.3);
```

#### Predefined Font Sizes

Use `PdfFontSize` constants for consistent typography:

```csharp
using PDFiumZ.HighLevel;

PdfFontSize.VerySmall  // 6pt
PdfFontSize.Small      // 8pt
PdfFontSize.Normal     // 10pt
PdfFontSize.Default    // 12pt
PdfFontSize.Medium     // 14pt
PdfFontSize.Large      // 16pt
PdfFontSize.Heading4   // 18pt
PdfFontSize.Heading3   // 20pt
PdfFontSize.Heading2   // 24pt
PdfFontSize.Heading1   // 28pt
PdfFontSize.Title      // 48pt
PdfFontSize.Giant      // 72pt
```

#### Fluent Content Creation

Create complex PDF content with chainable methods:

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();
using var page = document.CreatePage();
using var font = PdfFont.LoadStandardFont(document, PdfStandardFont.Helvetica);

using (var editor = page.BeginEdit())
{
    editor
        // Set default styles
        .WithFont(font)
        .WithFontSize(PdfFontSize.Heading1)
        .WithTextColor(PdfColor.DarkBlue)
        .Text("Title", 50, 750)  // Simplified 3-parameter Text method

        // Change styles and continue
        .WithFontSize(PdfFontSize.Normal)
        .WithTextColor(PdfColor.Black)
        .Text("Body text with default font and size", 50, 700)

        // Draw colored rectangles
        .WithStrokeColor(PdfColor.Red)
        .WithFillColor(PdfColor.WithOpacity(PdfColor.Red, 0.3))
        .Rectangle(new PdfRectangle(50, 630, 100, 50))

        // Draw lines
        .WithLineWidth(2)
        .Line(50, 600, 250, 600, PdfColor.Black)

        // Draw circles and ellipses
        .Circle(100, 550, 30, PdfColor.Blue, PdfColor.WithOpacity(PdfColor.Blue, 0.5))
        .Ellipse(new PdfRectangle(200, 520, 80, 60), PdfColor.Green, PdfColor.Transparent)

        // Use hex colors
        .Rectangle(new PdfRectangle(50, 450, 60, 30),
            PdfColor.FromHex("#FF6B6B"),
            PdfColor.WithOpacity(PdfColor.FromHex("#FF6B6B"), 0.5))

        // Commit all changes
        .Commit();
}

document.SaveToFile("fluent-content.pdf");
```

#### Configuration Methods

Set default styles for subsequent operations:

- `WithFont(font)` - Set default font
- `WithFontSize(fontSize)` - Set default font size
- `WithTextColor(color)` - Set default text color
- `WithStrokeColor(color)` - Set default stroke color for shapes
- `WithFillColor(color)` - Set default fill color for shapes
- `WithLineWidth(width)` - Set default line width

#### Shape Methods

Draw various shapes with ease:

- `Line(x1, y1, x2, y2, color, width)` - Draw a straight line
- `Rectangle(bounds)` - Draw rectangle with default colors
- `Rectangle(bounds, strokeColor, fillColor)` - Draw rectangle with custom colors
- `Circle(centerX, centerY, radius, strokeColor, fillColor)` - Draw a circle
- `Ellipse(bounds, strokeColor, fillColor)` - Draw an ellipse

#### Simplified Text Method

After setting a default font with `WithFont()`, use the simplified 3-parameter `Text()` method:

```csharp
editor
    .WithFont(font)
    .WithFontSize(14)
    .Text("Hello", 50, 700)  // Uses default font and size
    .Text("World", 50, 680); // Still uses default font and size
```

### HTML to PDF Conversion

PDFiumZ includes a built-in HTML to PDF converter that supports basic HTML tags and inline CSS styles.

#### Supported HTML Tags

- **Headings**: `<h1>`, `<h2>`, `<h3>`, `<h4>`, `<h5>`, `<h6>`
- **Paragraphs**: `<p>`, `<div>`
- **Text formatting**: `<b>`, `<strong>`, `<i>`, `<em>`, `<u>`, `<span>`
- **Layout**: `<br>` (line break)

#### Supported CSS Properties (inline styles)

- **font-size**: `10pt`, `12px`, `1.5em`
- **color**: Named colors (`red`, `blue`) or hex (`#FF0000`, `#F00`)
- **text-align**: `left`, `center`, `right`
- **font-weight**: `bold`, `normal`, or numeric (>=600 = bold)
- **font-style**: `italic`, `normal`
- **text-decoration**: `underline`

#### Basic Usage

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();

string html = @"
    <h1>Welcome to PDFiumZ</h1>
    <p>This is a simple HTML to PDF conversion example.</p>
";

document.CreatePageFromHtml(html);
document.SaveToFile("from-html.pdf");
```

#### With Inline Styles

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();

string html = @"
    <h1 style='color: #0066CC; text-align: center;'>Styled Heading</h1>
    <p style='font-size: 14pt; text-align: center;'>
        This paragraph has custom font size and is centered.
    </p>

    <h2 style='color: #FF6600;'>Text Formatting</h2>
    <p>
        This is <b>bold text</b>, this is <i>italic text</i>,
        and this is <u>underlined text</u>.
    </p>
    <p>You can also <b><i>combine styles</i></b>.</p>

    <h3 style='color: #009933;'>Colors</h3>
    <p style='color: red;'>This text is red.</p>
    <p style='color: #6A0DAD;'>This text uses a hex color.</p>
";

document.CreatePageFromHtml(html);
document.SaveToFile("styled-html.pdf");
```

#### With Custom Margins

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();

string html = "<h1>Custom Margins</h1><p>This page has custom margins.</p>";

// CreatePageFromHtml(html, marginLeft, marginRight, marginTop, marginBottom, pageWidth, pageHeight)
document.CreatePageFromHtml(html, 30, 30, 40, 40, 595, 842);
document.SaveToFile("custom-margins.pdf");
```

#### Complex Example

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();

string html = @"
    <h1 style='color: #2C3E50; text-align: center;'>PDFiumZ HTML Converter</h1>
    <p style='text-align: center; color: #7F8C8D; font-size: 11pt;'>
        A simple but powerful HTML to PDF converter
    </p>
    <br/>

    <h2 style='color: #E74C3C;'>Supported Tags</h2>
    <p><b>Headings:</b> h1, h2, h3, h4, h5, h6</p>
    <p><b>Text:</b> p, div, span, b, strong, i, em, u</p>
    <p><b>Layout:</b> br (line break)</p>
    <br/>

    <h2 style='color: #3498DB;'>Supported CSS Properties</h2>
    <p><b>font-size:</b> 10pt, 12px, 1.5em</p>
    <p><b>color:</b> red, #FF0000</p>
    <p><b>text-align:</b> left, center, right</p>
    <p><b>font-weight:</b> bold, normal</p>
    <p><b>font-style:</b> italic, normal</p>
    <p><b>text-decoration:</b> underline</p>
    <br/>

    <h3 style='color: #27AE60; text-align: center;'>
        Thank you for using PDFiumZ!
    </h3>
";

document.CreatePageFromHtml(html);
document.SaveToFile("complex-html.pdf");
```

#### Limitations

- No support for external CSS files (only inline styles)
- No support for images, tables, lists (ul, ol, li)
- No support for advanced layouts (flexbox, grid)
- Text wrapping is basic (no word break support)
- Single page output (no automatic page breaks)

For complex HTML rendering, consider using a dedicated HTML to PDF library like wkhtmltopdf or headless browsers.

## Features

### High-Level API (Recommended)

```csharp
using PDFiumZ;
using PDFiumZ.HighLevel;

PdfiumLibrary.Initialize();

try
{
    using var document = PdfDocument.Open("sample.pdf");
    Console.WriteLine($"Pages: {document.PageCount}");

    using var page = document.GetPage(0);
    using var image = page.RenderToImage();
    image.SaveAsSkiaPng("output.png");

    var text = page.ExtractText();
    Console.WriteLine(text);

    document.InsertBlankPage(0, 595, 842);
    document.DeletePage(2);
    document.MovePages(new[] { 0, 1 }, 3);

    using var source = PdfDocument.Open("other.pdf");
    document.ImportPages(source, "1,3,5-7");

    document.SaveToFile("modified.pdf");
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

### Feature Checklist

- ✅ Modern C# API with `IDisposable` pattern
- ✅ Fluent rendering options (`WithDpi()`, `WithScale()`, `WithTransparency()`)
- ✅ **Enhanced fluent API for content creation**:
  - Configuration methods (`WithFont`, `WithFontSize`, `WithTextColor`, `WithStrokeColor`, `WithFillColor`, `WithLineWidth`)
  - Simplified text method using defaults (3-parameter `Text()`)
  - Shape drawing (`Line`, `Circle`, `Ellipse`, enhanced `Rectangle`)
  - Predefined colors (`PdfColor` with 40+ colors)
  - Common font sizes (`PdfFontSize` with 15+ presets)
  - Hex color support and opacity control
- ✅ **HTML to PDF conversion**:
  - Support for common HTML tags (h1-h6, p, div, span, b, strong, i, em, u, br)
  - Inline CSS styles (font-size, color, text-align, font-weight, font-style, text-decoration)
  - Extension method (`CreatePageFromHtml`)
  - Customizable margins
- ✅ PDF creation from scratch
- ✅ PDF merge and split
- ✅ Page manipulation (insert, delete, move, import)
- ✅ Page rotation
- ✅ Text watermarks
- ✅ Bookmarks
- ✅ Form fields (read/write)
- ✅ Document metadata
- ✅ Page labels
- ✅ Hyperlink detection
- ✅ Text extraction and search
- ✅ Image extraction
- ✅ Page object enumeration
- ✅ Document saving
- ✅ Zero-copy image access via `Span<byte>`
- ✅ Annotations (highlight, underline, strikeout, notes, stamps, line, square, circle, ink, free text)
- ✅ Content creation (text, images, shapes)
- ✅ Font management (standard + TrueType)
- ✅ Async API with cancellation support
- ✅ Batch operations

### Low-Level API

The low-level P/Invoke API is available through the `fpdfview`, `fpdf_edit`, `fpdf_doc` classes.

## Building

### Requirements

- .NET 10.0 SDK or later

### Build and Pack

From the repo root:

```bash
dotnet build src/PDFiumZ.sln -c Release
dotnet pack src/PDFiumZ/PDFiumZ.csproj -c Release
```

Build a single target framework:

```bash
dotnet build src/PDFiumZ/PDFiumZ.csproj -c Release -f netstandard2.0
dotnet build src/PDFiumZ/PDFiumZ.csproj -c Release -f net9.0
```

Compatibility notes:

- `netstandard2.0` builds pull in `System.Memory` to provide `Span<T>` support
- `netstandard2.x` builds include an internal `IsExternalInit` shim for `init`/`record` support

## Benchmarks

```bash
cd src/PDFiumZ.Benchmarks
dotnet run -c Release
```

For benchmark documentation and performance tips, see `src/PDFiumZ.Benchmarks/README.md`. For the performance analysis report, see `PERFORMANCE.md`.

## Support

<div align="center">
  <img src="./wechat-pay-qr.png" width="200" alt="WeChat Pay">
  <img src="./alipay-qr.png" width="200" alt="Alipay">
</div>
