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

## Documentation

- Examples and extended snippets: `docs/README.md`
- Performance analysis: `PERFORMANCE.md`
- Benchmarks: `src/PDFiumZ.Benchmarks/README.md`
- Changelog: `CHANGELOG.md`
- Roadmap: `ROADMAP.md`

## Building

- Requirements: .NET 10.0 SDK or later
- Build: `dotnet build src/PDFiumZ.sln -c Release`
- Pack: `dotnet pack src/PDFiumZ/PDFiumZ.csproj -c Release`

## License

Apache-2.0. See `LICENSE`.

## Support

See `docs/README.md#support`.

