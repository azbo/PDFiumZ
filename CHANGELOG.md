# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **High-Level API** - New `PDFiumCore.HighLevel` namespace with modern, easy-to-use API
  - `PdfDocument` - Document lifecycle management with factory methods (`Open`, `OpenFromMemory`)
  - `PdfPage` - Page operations with rendering and text extraction
  - `PdfImage` - Rendered page as bitmap with zero-copy buffer access
  - `RenderOptions` - Fluent API for rendering configuration (DPI, scale, transparency, flags)
  - `PdfiumLibrary` - Static library initialization with thread-safe reference counting
  - Exception hierarchy: `PdfException`, `PdfLoadException`, `PdfPasswordException`, `PdfRenderException`

- **Page Manipulation Features**
  - `InsertBlankPage()` - Create and insert blank pages at any position
  - `DeletePage()` - Remove pages from document
  - `MovePages()` - Rearrange pages within document
  - `ImportPages()` - Copy pages from another document using page range strings (e.g., "1,3,5-7")
  - `ImportPagesAt()` - Copy specific page indices from another document
  - `SaveToFile()` - Save modified documents to disk

- **Bookmark Operations**
  - `PdfBookmark` - Bookmark navigation with tree traversal support
    - Navigate bookmark hierarchy (first child, next sibling)
    - Get bookmark title and destination
    - Enumerate children and descendants recursively
  - `PdfDestination` - Bookmark destination with page index
  - `GetFirstBookmark()` - Get first root-level bookmark
  - `GetBookmarks()` - Enumerate all root-level bookmarks
  - `FindBookmark()` - Search bookmarks by title

- **Form Field Operations**
  - `PdfFormField` - Form field annotation with read-only access
    - Read field properties: Name, AlternateName, Value, FieldType, Flags
    - Check checkbox/radio button state: IsChecked
    - Access combo box/list box options: GetAllOptions(), GetOptionLabel(), IsOptionSelected()
  - `PdfFormFieldType` - Enumeration of 7 standard field types (TextField, CheckBox, RadioButton, ComboBox, ListBox, PushButton, Signature)
  - `GetFormFieldCount()` - Get count of form fields on page
  - `GetFormField(index)` - Get specific form field
  - `GetFormFields()` - Enumerate all form fields on page

- **Document Metadata**
  - `PdfMetadata` - Document metadata class with standard PDF properties
    - Title, Author, Subject, Keywords - Basic document information
    - Creator, Producer - Application information
    - CreationDate, ModificationDate - Timestamp information in PDF format
  - `PdfDocument.Metadata` - Property to access document metadata with lazy loading

- **Page Labels**
  - `GetPageLabel(index)` - Get custom page label for specific page
  - `GetAllPageLabels()` - Get all page labels as dictionary
  - Supports custom page numbering (e.g., "i", "ii", "iii" for Roman numerals)
  - Returns numeric page number (1-based) if no custom label defined

- **Hyperlink Support**
  - `PdfLink` - Hyperlink class with destination access
  - `PdfLinkDestination` - Link destination (internal page or external URI)
  - `PdfLinkDestinationType` - Enum for destination types (InternalPage, ExternalUri, Unknown)
  - `PdfPage.GetLinkAtPoint(x, y)` - Detect link at specific coordinate

- **Demo Application Updates**
  - Added comprehensive examples for all high-level API features
  - Demonstrates rendering, text extraction, and page manipulation
  - Shows proper resource management patterns

### Changed
- Target framework updated to .NET 10.0
- Demo project migrated from ImageSharp to SkiaSharp
  - Zero-copy integration with PDFium bitmaps
  - Extension methods in `PDFiumCore.SkiaSharp` namespace
  - Support for PNG and JPEG export

### Technical Details
- All high-level classes implement `IDisposable` for proper resource management
- Internal constructors prevent invalid object creation
- Comprehensive XML documentation on all public APIs
- Unsafe contexts used where needed for optimal performance
- 100% backward compatibility - original P/Invoke API unchanged

## [145.0.7578.0] - 2025-12-20

### Changed
- **Upgraded PDFium from 143.0.7469.0 to 145.0.7578.0** (chromium/7578)
  - Includes significantly improved text reading capabilities
  - Better handling of complex PDF text extraction
  - Enhanced support for special characters and multilingual text
- Updated target framework from .NET Standard 2.1 to .NET 8.0+
  - Removed support for older frameworks (netstandard2.1, net5.0, net6.0)
  - Simplified package dependencies
- Updated Clang toolchain from 14.0.0 to 18.0
- Updated build tools to .NET 9.0

### Removed
- Removed `System.Runtime.CompilerServices.Unsafe` package dependency (no longer needed in .NET 8.0)
- Removed support for older .NET target frameworks

## [143.0.7469.0] - Previous Release

### Note
- Previous releases were not documented in this CHANGELOG
- Version history available through git commit history
