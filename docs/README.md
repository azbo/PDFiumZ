# PDFiumZ Documentation

## Contents

- [Examples](#examples)
- [Features](#features)
- [Building](#building)
- [Benchmarks](#benchmarks)
- [Support](#support)

## Examples

### Create PDF from Scratch

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();

using var page1 = document.CreatePage(595, 842);   // A4 (210mm x 297mm)
using var page2 = document.CreatePage(612, 792);   // Letter (8.5" x 11")
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
