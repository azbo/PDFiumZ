# <img src="./src/PDFiumZ/icon.png" width="48"> PDFiumZ [![NuGet](https://img.shields.io/nuget/v/PDFiumZ.svg?maxAge=60)](https://www.nuget.org/packages/PDFiumZ)

PDFiumZ is a modern .NET wrapper for [PDFium](https://pdfium.googlesource.com/pdfium/) with a comprehensive high-level API. Built on [PDFium binaries](https://github.com/bblanchon/pdfium-binaries) with P/Invoke bindings. Supports Linux-x64, OSX-x64, Win-x64, Win-x86.

**Current PDFium Version: 145.0.7592.0** (chromium/7592) - Latest PDFium with enhanced rendering and stability

## Documentation

- 🚀 **[Quick Start & Examples](./docs/README.md)** - Get up and running in minutes
- 🖼️ **[Image Generation Guide](./docs/IMAGE_GENERATION.md)** - Convert PDF pages to images with one line of code
- 📝 **[Fluent API Guide](./docs/FLUENT_API.md)** - QuestPDF-style declarative document generation
- 📈 **[Performance Analysis](./PERFORMANCE.md)** - Memory and speed benchmarks
- 📜 **[Changelog](./CHANGELOG.md)** - Recent updates and breaking changes
- 🗺️ **[Roadmap](./ROADMAP.md)** - Future plans and upcoming features

## Supported Frameworks

PDFiumZ is multi-targeted:

- `net10.0`, `net9.0`, `net8.0`
- `netstandard2.1`, `netstandard2.0`

## Installation

```bash
dotnet add package PDFiumZ
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

## Key Features

### Core Functionality
- **High-Level API**: Easy to use classes for documents, pages, annotations, and forms
- **Cross-Platform**: Works on Windows, Linux, and macOS
- **Async Support**: Modern async/await APIs for I/O bound operations
- **Multi-Framework**: Supports .NET 10/9/8 and .NET Standard 2.0/2.1

### Document Operations
- **Create PDFs**: Generate new PDF documents from scratch
- **Merge & Split**: Combine multiple PDFs or extract specific pages
- **Rotate Pages**: Rotate individual pages or entire documents (90°, 180°, 270°)
- **Watermarks**: Add text watermarks with custom opacity, rotation, and position
- **Security Info**: Read encryption status and document permissions

### Content Creation & Editing
- **Content Editor**: Add text, images, shapes, and paths with a powerful fluent API
- **Fluent Document Generation**: QuestPDF-style declarative API for complex layouts
- **HTML to PDF**: Convert simple HTML/CSS to PDF documents
- **Table Builder**: Create complex tables with fluent API

### Annotations (10+ Types)
- **Text Markup**: Highlight, Underline, StrikeOut
- **Shapes**: Square, Circle, Line
- **Text**: Sticky Notes, Free Text boxes
- **Drawing**: Ink (freehand), Stamps
- **Form Fields**: Read and write form field values

### Rendering & Extraction
- **Image Generation**: Simple, high-performance PDF to image conversion
- **Thumbnail Generation**: Generate page thumbnails with customizable size and quality ✨ **NEW**
- **Text Extraction**: Extract plain text with positions and formatting
- **Image Extraction**: Extract embedded images from PDF pages
- **Search**: Find text with position information

## Building

Requirements: .NET 10.0 SDK or later.

```bash
dotnet build src/PDFiumZ.sln -c Release
```

## License

Apache-2.0. See `LICENSE`.

## Support

如果这个项目对您有帮助，欢迎赞助支持！
If this project helps you, please consider sponsoring!

<div align="center">
  <img src="docs/wechat-pay-qr.png" width="200" alt="WeChat Pay">
  <img src="docs/alipay-qr.png" width="200" alt="Alipay">
</div>
