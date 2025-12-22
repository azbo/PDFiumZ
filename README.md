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

See `docs/README.md#support`.

<div align="center">
  <img src="./docs/wechat-pay-qr.png" width="200" alt="WeChat Pay">
  <img src="./docs/alipay-qr.png" width="200" alt="Alipay">
</div>

