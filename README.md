# <img src="./src/PDFiumCore/icon.png" width="48"> PDFiumCore [![NuGet](https://img.shields.io/nuget/v/PDFiumCore.svg?maxAge=60)](https://www.nuget.org/packages/PDFiumCore) [![Action Workflow](https://github.com/Dtronix/PDFiumCore/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Dtronix/PDFiumCore/actions)

PDFiumCore is a .NET 8.0+ wrapper for the [PDFium](https://pdfium.googlesource.com/pdfium/) library which includes the [binaries](https://github.com/bblanchon/pdfium-binaries) and header pinvoke bindings.  Supports Linux-x64, OSX-x64, Win-x64, Win-x86.

**Current PDFium Version: 145.0.7578.0** (chromium/7578) - Includes improved text reading capabilities

Bindings are generated from the binaries and header files created at [pdfium-binaries](https://github.com/bblanchon/pdfium-binaries) repository.

### Usage

The preferred way to use this project is to use the [Nuget Package](https://www.nuget.org/packages/PDFiumCore).  This will ensure all the proper bindings in the `*.deps.json` are generated and included for the targeted environments.

#### High-Level API (Recommended)

PDFiumCore now includes a high-level API (`PDFiumCore.HighLevel` namespace) that provides a modern, easy-to-use interface with automatic resource management:

```csharp
using PDFiumCore;
using PDFiumCore.HighLevel;

// Initialize library (call once at application start)
PdfiumLibrary.Initialize();

try
{
    // Open document with automatic resource management
    using var document = PdfDocument.Open("sample.pdf");
    Console.WriteLine($"Pages: {document.PageCount}");

    // Render a page to image
    using var page = document.GetPage(0);
    using var image = page.RenderToImage();

    // Save as PNG (requires PDFiumCore.SkiaSharp extension package)
    image.SaveAsSkiaPng("output.png");

    // Extract text
    var text = page.ExtractText();
    Console.WriteLine(text);

    // Page manipulation
    document.InsertBlankPage(0, 595, 842);  // Insert A4 page at beginning
    document.DeletePage(2);                  // Delete page 2
    document.MovePages(new[] { 0, 1 }, 3);  // Move pages to new position

    // Import pages from another document
    using var source = PdfDocument.Open("other.pdf");
    document.ImportPages(source, "1,3,5-7");  // Import specific pages

    // Save modified document
    document.SaveToFile("modified.pdf");

    // Navigate bookmarks
    foreach (var bookmark in document.GetBookmarks())
    {
        Console.WriteLine($"{bookmark.Title} -> Page {bookmark.Destination?.PageIndex}");

        // Recursively iterate children
        foreach (var child in bookmark.GetAllDescendants())
        {
            Console.WriteLine($"  {child.Title}");
        }
    }
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

**Features:**
- ✅ Modern C# API with `IDisposable` pattern
- ✅ Fluent rendering options (`WithDpi()`, `WithScale()`, `WithTransparency()`)
- ✅ Page manipulation (insert, delete, move, import)
- ✅ Bookmark navigation with tree traversal
- ✅ Text extraction
- ✅ Document saving
- ✅ Zero-copy image access via `Span<byte>`

#### Low-Level API

The low-level P/Invoke API is still available for advanced scenarios through the `fpdfview`, `fpdf_edit`, `fpdf_doc` classes.

### Build Requirements
- .NET 8.0+

### Manual Building 

Execute the CreateBindingsPackage.bat

This will do the following:
 - Download the specified files at the passed pdfium-binaries API url.
 - Extracts the zip & tgz files into the `asset/libraries`directory.
 - Opens the pdfium-windows-x64 directory and parses the header files via CppSharp and generates ``PDFiumCore.cs`` in the current directory.
 - Copies the libraries and licenses into their respective ``src/PDFiumCore/runtimes`` directories.
 - Copies/Overrides ``src/PDFiumCore/PDFiumCore.cs`` with the newly generated ``PDFiumCore.cs``.

##### PDFiumCoreBindingsGenerator Parameters

PDFiumCoreBindingsGenerator.exe requires the following parameters:

 - [0] Set to either a specific Github API release ID for the `bblanchon/pdfium-binaries` project or `latest`. This is to determine the release version and binary assets to download.
 - [1] Set to true to download the libraries and generate the bindings.  Set to false to only download the libraries.
 - [2] Version to set the Version.Revision property to.  This is used for building patches. Usually set to "0"


### ToDo
 - Create an actual parser for the comments and generate functional C# method documentation.
 - Include documentation for more than just the public methods.
 - Investigate ARM builds for inclusion.

### Resources

https://pdfium.googlesource.com/pdfium/

https://github.com/bblanchon/pdfium-binaries

https://github.com/mono/CppSharp

### License
Matching the PDFium project, this project is released under [Apache-2.0 License](LICENSE).

### Sponsor / 赞助

If this project helps you, consider buying me a coffee! 如果这个项目对你有帮助，请考虑支持一下！

<img src="./src/PDFiumCore/收款.jpg" width="200">
