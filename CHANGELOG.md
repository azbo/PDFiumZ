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
