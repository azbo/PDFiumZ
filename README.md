# <img src="./src/PDFiumZ/icon.png" width="48"> PDFiumZ [![NuGet](https://img.shields.io/nuget/v/PDFiumZ.svg?maxAge=60)](https://www.nuget.org/packages/PDFiumZ)

PDFiumZ is a modern .NET wrapper for [PDFium](https://pdfium.googlesource.com/pdfium/) with a comprehensive high-level API. Built on [PDFium binaries](https://github.com/bblanchon/pdfium-binaries) with P/Invoke bindings. Supports Linux-x64, OSX-x64, Win-x64, Win-x86.

**Current PDFium Version: 145.0.7578.0** (chromium/7578) - Includes improved text reading capabilities

## Supported Frameworks

PDFiumZ is multi-targeted:

- `net10.0`
- `net9.0`
- `net8.0`
- `netstandard2.1`
- `netstandard2.0`

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

## Common Examples

All snippets assume you call `PdfiumLibrary.Initialize()` once at app start and `PdfiumLibrary.Shutdown()` on exit.

### Create a New Document

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();
using var page1 = document.CreatePage();
using var page2 = document.CreatePage(PdfPageSize.A3);

document.SaveToFile("new-document.pdf");
```

### Merge and Split

```csharp
using PDFiumZ.HighLevel;

using var merged = PdfDocument.Merge("a.pdf", "b.pdf");
merged.SaveToFile("merged.pdf");

using var source = PdfDocument.Open("merged.pdf");
using var first3 = source.Split(0, 3);
first3.SaveToFile("first-3-pages.pdf");
```

### Render and Extract Text

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);

using var image = page.RenderToImage(RenderOptions.Default.WithDpi(150));
image.SaveAsSkiaPng("page-0.png");

var text = page.ExtractText();
```

### Rotate Pages

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");
document.RotateAllPages(PdfRotation.Rotate90);
document.SaveToFile("rotated.pdf");
```

### Add a Watermark

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");
document.AddTextWatermark("CONFIDENTIAL", WatermarkPosition.Center, new WatermarkOptions { Opacity = 0.3, Rotation = 45 });
document.SaveToFile("watermarked.pdf");
```

### Add Header and Footer

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");

document.AddHeaderFooter(
    headerText: "My Report — Page {page}/{pages}",
    footerText: "Confidential",
    options: new HeaderFooterOptions { FontSize = 10, Margin = 36 });

document.SaveToFile("with-header-footer.pdf");
```

### Create Content (Fluent API)

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();
using var page = document.CreatePage();
using var font = PdfFont.LoadStandardFont(document, PdfStandardFont.Helvetica);

using (var editor = page.BeginEdit())
{
    editor
        // Set defaults and use simplified methods
        .WithFont(font)
        .WithFontSize(PdfFontSize.Heading1)
        .WithTextColor(PdfColor.DarkBlue)
        .Text("Enhanced Fluent API", 50, 750)

        // Draw colored shapes
        .WithStrokeColor(PdfColor.Red)
        .WithFillColor(PdfColor.WithOpacity(PdfColor.Red, 0.3))
        .Rectangle(new PdfRectangle(50, 680, 100, 50))

        // Draw lines and circles
        .WithLineWidth(2)
        .Line(50, 650, 250, 650, PdfColor.Black)
        .Circle(100, 600, 30, PdfColor.Blue, PdfColor.WithOpacity(PdfColor.Blue, 0.5))

        // Use hex colors
        .Rectangle(new PdfRectangle(50, 520, 60, 30),
            PdfColor.FromHex("#FF6B6B"),
            PdfColor.WithOpacity(PdfColor.FromHex("#FF6B6B"), 0.5))

        .Commit();
}

document.SaveToFile("content.pdf");
```

### Create Tables (Fluent API)

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();
using var page = document.CreatePage();
using var font = PdfFont.LoadStandardFont(document, PdfStandardFont.Helvetica);

using (var editor = page.BeginEdit())
{
    editor
        .WithFont(font)
        .WithFontSize(PdfFontSize.Heading1)
        .WithTextColor(PdfColor.DarkBlue)
        .Text("Employee Directory", 50, 780);

    // Build table with fluent API
    editor.BeginTable()
        .Columns(cols => cols
            .Add(120)   // Fixed: 120pt
            .Add()      // Auto width (equal distribution)
            .Add(100)   // Fixed: 100pt
            .Add(80))   // Fixed: 80pt
        .HeaderBackgroundColor(PdfColor.WithOpacity(PdfColor.Blue, 0.2))
        .HeaderTextColor(PdfColor.DarkBlue)
        .HeaderFontSize(14)
        .CellPadding(8)
        .BorderWidth(1)
        .BorderColor(PdfColor.Gray)
        .Header(header => header
            .Cell("Name")
            .Cell("Position")
            .Cell("Department")
            .Cell("Ext"))
        .Row(row => row
            .Cell("John Doe")
            .Cell("Senior Developer")
            .Cell("Engineering")
            .Cell("1234"))
        .Row(row => row
            .Cell("Jane Smith")
            .Cell("Product Manager")
            .Cell("Product")
            .Cell("5678"))
        .Row(row => row
            .Cell("Bob Johnson")
            .Cell("QA Engineer")
            .Cell("Quality")
            .Cell("9012"))
        .EndTable();

    editor.Commit();
}

document.SaveToFile("table.pdf");
```

### SkiaSharp Integration

```csharp
using PDFiumZ.HighLevel;
using PDFiumZ.SkiaSharp;  // Extension methods
using SkiaSharp;

// Render PDF to various image formats
using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);
using var image = page.RenderToImage();

image.SaveAsSkiaPng("output.png");
image.SaveAsSkiaJpeg("output.jpg", quality: 90);
image.SaveAsSkiaWebP("output.webp", quality: 85);

// Add images to PDF from SkiaSharp
using var newDoc = PdfDocument.CreateNew();
using var newPage = newDoc.CreatePage();
using var editor = newPage.BeginEdit();

// Load and add image from file
editor.AddImageFromFile("chart.png", new PdfRectangle(50, 700, 300, 200));

// Or use SKBitmap/SKImage directly
using var bitmap = SKBitmap.Decode("photo.jpg");
editor.AddSkiaImage(bitmap, new PdfRectangle(50, 450, 200, 150));

editor.Commit();
newDoc.SaveToFile("with-images.pdf");
```

### Convert HTML to PDF

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();

string html = @"
    <h1 style='color: #0066CC; text-align: center;'>Welcome to PDFiumZ</h1>
    <p style='font-size: 14pt;'>Convert HTML to PDF with ease!</p>

    <h2 style='color: #FF6600;'>Features</h2>
    <p>Supports <b>bold</b>, <i>italic</i>, and <u>underline</u> text.</p>
    <p style='text-align: center; color: #009933;'>Centered and colored text.</p>

    <h2>Lists</h2>
    <ul>
        <li>Unordered lists with bullet points</li>
        <li>Nested lists support
            <ul>
                <li>Depth-based bullet styles</li>
                <li>Unlimited nesting levels</li>
            </ul>
        </li>
    </ul>

    <h2>Tables</h2>
    <table border='2' cellpadding='8'>
        <tr>
            <th>Product</th>
            <th>Price</th>
            <th>Stock</th>
        </tr>
        <tr>
            <td>Widget A</td>
            <td>$19.99</td>
            <td>50</td>
        </tr>
        <tr>
            <td>Widget B</td>
            <td>$29.99</td>
            <td>30</td>
        </tr>
    </table>
";

document.CreatePageFromHtml(html);
document.SaveToFile("from-html.pdf");
```

## Documentation
```

## Documentation

- Examples and extended snippets: `docs/README.md`
- Performance analysis: `PERFORMANCE.md`
- Benchmarks: `src/PDFiumZ.Benchmarks/README.md`
- Changelog: `CHANGELOG.md`
- Roadmap: `ROADMAP.md`

## Completed

- Create PDF from scratch (`PdfDocument.CreateNew`, `CreatePage`)
- Merge / split documents (`PdfDocument.Merge`, `Split`)
- Watermarks (`AddTextWatermark`)
- Page rotation (single / batch / all pages)
- Text extraction and search (with positions)
- Render to bitmap and save as images (via extension packages)
- Annotation support (highlight, underline, strikeout, notes, stamps, line, square, circle, ink, free text)
- Content creation (add text/images/shapes) and font management
- **Enhanced fluent API** with default styles, colors, and shapes:
  - Configuration methods (`WithFont`, `WithFontSize`, `WithTextColor`, `WithStrokeColor`, `WithFillColor`, `WithLineWidth`)
  - Simplified methods (`Text` with 3 parameters using defaults)
  - Shape drawing (`Line`, `Circle`, `Ellipse`, enhanced `Rectangle`)
  - Predefined colors (`PdfColor` - 40+ colors including basic, extended, highlights, and shades)
  - Common font sizes (`PdfFontSize` - 15+ presets from 6pt to 72pt)
  - Hex color support (`PdfColor.FromHex`, `WithOpacity`)
- **HTML to PDF conversion**:
  - Support for common HTML tags (h1-h6, p, div, span, b, strong, i, em, u, br)
  - Inline CSS styles (font-size, color, text-align, font-weight, font-style, text-decoration)
  - Simple extension method (`CreatePageFromHtml`)
  - Customizable margins
- Security info reading (encryption + permissions)
- Async APIs + batch operations
- Multi-targeting: `net10.0`, `net9.0`, `net8.0`, `netstandard2.1`, `netstandard2.0`

See `CHANGELOG.md` and `ROADMAP.md` for full details.

## Performance

- Performance test report: `PERFORMANCE.md`
- Benchmarks guide and how to run: `src/PDFiumZ.Benchmarks/README.md`

## Building

- Requirements: .NET 10.0 SDK or later
- Build: `dotnet build src/PDFiumZ.sln -c Release`
- Pack: `dotnet pack src/PDFiumZ/PDFiumZ.csproj -c Release`

## License

Apache-2.0. See `LICENSE`.

## Support

如果这个项目对您有帮助，欢迎赞助支持！

If this project helps you, please consider sponsoring!

<div align="center">
  <img src="./docs/wechat-pay-qr.png" width="200" alt="WeChat Pay">
  <img src="./docs/alipay-qr.png" width="200" alt="Alipay">
</div>

