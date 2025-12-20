# <img src="./src/PDFiumZ/icon.png" width="48"> PDFiumZ [![NuGet](https://img.shields.io/nuget/v/PDFiumZ.svg?maxAge=60)](https://www.nuget.org/packages/PDFiumZ)

PDFiumZ is a modern .NET 10.0+ wrapper for [PDFium](https://pdfium.googlesource.com/pdfium/) with a comprehensive high-level API. Built on [PDFium binaries](https://github.com/bblanchon/pdfium-binaries) with P/Invoke bindings. Supports Linux-x64, OSX-x64, Win-x64, Win-x86.

**Current PDFium Version: 145.0.7578.0** (chromium/7578) - Includes improved text reading capabilities

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

## Features

### High-Level API (Recommended)

PDFiumZ provides a modern, fluent API that makes PDF operations simple and intuitive:

```csharp
using PDFiumZ;
using PDFiumZ.HighLevel;

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

    // Save as PNG (requires PDFiumZ.SkiaSharp extension package)
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

    // Access document metadata
    var meta = document.Metadata;
    Console.WriteLine($"Title: {meta.Title}");
    Console.WriteLine($"Author: {meta.Author}");
    Console.WriteLine($"Producer: {meta.Producer}");
    Console.WriteLine($"Creation Date: {meta.CreationDate}");

    // Read form fields
    using var formPage = document.GetPage(0);
    foreach (var field in formPage.GetFormFields())
    {
        using (field)
        {
            Console.WriteLine($"Field: {field.Name}");
            Console.WriteLine($"Type: {field.FieldType}");
            Console.WriteLine($"Value: {field.Value}");

            if (field.FieldType == PdfFormFieldType.CheckBox)
                Console.WriteLine($"Checked: {field.IsChecked}");

            if (field.FieldType == PdfFormFieldType.ComboBox)
            {
                var options = field.GetAllOptions();
                Console.WriteLine($"Options: {string.Join(", ", options)}");
            }
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
- ✅ Form field reading (all standard field types)
- ✅ Document metadata access (title, author, dates, etc.)
- ✅ Page labels (custom page numbering)
- ✅ Hyperlink support (detect links at coordinates)
- ✅ Text extraction
- ✅ Text search with position information (case-sensitive, whole-word options)
- ✅ Image extraction from PDF pages
- ✅ Page object enumeration and type classification
- ✅ Document saving
- ✅ Zero-copy image access via `Span<byte>`

#### Low-Level API

The low-level P/Invoke API is still available for advanced scenarios through the `fpdfview`, `fpdf_edit`, `fpdf_doc` classes.

## Build Requirements
- .NET 10.0 SDK or later

## Building from Source

1. Clone the repository
2. Run `dotnet build` in the `src` directory
3. Run `dotnet pack` to create NuGet packages

### Resources

- [PDFium Source](https://pdfium.googlesource.com/pdfium/)
- [PDFium Binaries](https://github.com/bblanchon/pdfium-binaries)
- [CppSharp](https://github.com/mono/CppSharp)

### License
This project is released under [Apache-2.0 License](LICENSE), matching the PDFium project license.
