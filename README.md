# <img src="./src/PDFiumZ/icon.png" width="48"> PDFiumZ [![NuGet](https://img.shields.io/nuget/v/PDFiumZ.svg?maxAge=60)](https://www.nuget.org/packages/PDFiumZ)

PDFiumZ is a modern .NET wrapper for [PDFium](https://pdfium.googlesource.com/pdfium/) with a comprehensive high-level API. Built on [PDFium binaries](https://github.com/bblanchon/pdfium-binaries) with P/Invoke bindings. Supports Linux-x64, OSX-x64, Win-x64, Win-x86.

**Current PDFium Version: 145.0.7578.0** (chromium/7578) - Includes improved text reading capabilities

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

- **High-Level API**: Easy to use classes for documents, pages, annotations, and forms.
- **Content Creation**: Add text, images, and shapes with a powerful fluent editor.
- **Fluent Document Generation**: QuestPDF-style declarative API for complex layouts.
- **Image Generation**: Simple, high-performance PDF to image conversion.
- **HTML to PDF**: Convert simple HTML/CSS to PDF documents.
- **Annotation Support**: Comprehensive support for 10+ types of PDF annotations.
- **Cross-Platform**: Works on Windows, Linux, and macOS.
- **Async Support**: Modern async/await APIs for I/O bound operations.

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
