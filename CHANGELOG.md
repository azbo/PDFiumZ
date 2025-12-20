# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- **Project renamed to PDFiumZ** - Package ID and assembly name changed from PDFiumZ to PDFiumZ
- Namespaces remain as `PDFiumZ` and `PDFiumZ.HighLevel` for API compatibility
- Updated package description and metadata
- Reorganized documentation for clarity

### Added
- **High-Level API** - New `PDFiumZ.HighLevel` namespace with modern, easy-to-use API
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
  - `PdfFormField` - Form field annotation with read and write access
    - Read field properties: Name, AlternateName, Value, FieldType, Flags
    - Check checkbox/radio button state: IsChecked
    - Access combo box/list box options: GetAllOptions(), GetOptionLabel(), IsOptionSelected()
    - Write operations:
      - `SetValue(string)` - Set text field value
      - `SetChecked(bool)` - Set checkbox/radio button state
      - `SetSelectedOption(int)` - Set single selection for combo/list boxes
      - `SetSelectedOptions(int[])` - Set multiple selections for list boxes
  - `PdfFormFieldType` - Enumeration of 7 standard field types (TextField, CheckBox, RadioButton, ComboBox, ListBox, PushButton, Signature)
  - `GetFormFieldCount()` - Get count of form fields on page
  - `GetFormField(index)` - Get specific form field
  - `GetFormFields()` - Enumerate all form fields on page
  - `PdfDocument.HasForm` - Check if document contains interactive forms
  - `PdfDocument.FlattenForm()` - Convert interactive forms to static content

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

- **Text Search**
  - `PdfTextSearchResult` - Search result class with match position and bounding rectangles
  - `PdfPage.SearchText()` - Search for text on page with case-sensitive and whole-word options
  - Returns character indices, matched text, and bounding rectangles for each occurrence
  - Supports multi-line text matches with multiple bounding rectangles

- **Image Extraction**
  - `PdfExtractedImage` - Extracted image class with rendered bitmap and object index
  - `PdfPageObject` - Page object information with type classification
  - `PdfPageObjectType` - Enumeration of page object types (Text, Path, Image, Shading, Form)
  - `PdfPage.ExtractImages()` - Extract all images from page
  - `PdfPage.ExtractImage(index)` - Extract specific image by object index
  - `PdfPage.GetPageObjectCount()` - Get count of page objects
  - `PdfPage.GetPageObject(index)` - Get page object information
  - Extracted images can be saved using SkiaSharp extension methods

- **Annotation Support**
  - `PdfAnnotation` - Base class for PDF annotations with IDisposable pattern
  - `PdfAnnotationType` - Enumeration of 27 PDF annotation types
  - `PdfHighlightAnnotation` - Highlight annotation with quad points support
    - Create highlights with custom colors (ARGB format)
    - Set/Get quad points for multi-line highlighting
  - `PdfTextAnnotation` - Text annotation (sticky note)
    - Create text annotations at specific coordinates
    - Set/Get contents and author properties
  - `PdfStampAnnotation` - Stamp annotation (rubber stamp)
    - 14 standard stamp types (Draft, Approved, Confidential, etc.)
    - Custom positioning and sizing
  - `GenericAnnotation` - Fallback wrapper for unsupported annotation types
  - Annotation management methods on `PdfPage`:
    - `GetAnnotationCount()` - Get total annotation count
    - `GetAnnotation(index)` - Get annotation by index
    - `GetAnnotations()` - Enumerate all annotations
    - `GetAnnotations<T>()` - Filter annotations by type
    - `RemoveAnnotation(index)` - Remove annotation by index
    - `RemoveAnnotation(annotation)` - Remove specific annotation
  - Supports reading existing annotations and creating new ones
  - All annotation properties (color, bounds, type-specific data) accessible
  - Changes persist when document is saved

- **Content Creation** - Add text, images, and shapes to PDF pages
  - `PdfFont` - Font management with resource disposal
    - `LoadStandardFont(document, string)` - Load font by name
    - `LoadStandardFont(document, PdfStandardFont)` - Load font by enum
    - `LoadTrueTypeFont(document, byte[], bool)` - Load custom TrueType fonts
    - `PdfStandardFont` enum - 14 standard PDF fonts (Helvetica, Times, Courier, etc.)
  - `PdfContentEditor` - Content editing with IDisposable pattern
    - `AddText(text, x, y, font, fontSize)` - Add text at specified position
    - `AddImage(imageData, width, height, bounds)` - Add images from BGRA bitmap data
    - `AddRectangle(bounds, strokeColor, fillColor)` - Add rectangles with colors
    - `RemoveObject(index)` - Remove page objects by index
    - `GenerateContent()` - Persist changes to page content stream
  - `PdfPage` content editing methods:
    - `BeginEdit()` - Create content editor for page
    - `GenerateContent()` - Regenerate page content stream
  - Support for standard PDF fonts (14 built-in fonts that don't require embedding)
  - Support for custom TrueType fonts from byte arrays
  - ARGB color format for stroke and fill colors with alpha channel support
  - Proper resource management with automatic cleanup

- **Demo Application Updates**
  - Added comprehensive examples for all high-level API features
  - Demonstrates rendering, text extraction, and page manipulation
  - Added annotation management demo with all annotation types (highlight, text, stamps)
  - Shows annotation creation, reading, filtering, and removal operations
  - Added content creation demo with text and shapes
  - Demonstrates font loading and management
  - Shows proper resource management patterns

### Changed
- Target framework updated to .NET 10.0
- Demo project migrated from ImageSharp to SkiaSharp
  - Zero-copy integration with PDFium bitmaps
  - Extension methods in `PDFiumZ.SkiaSharp` namespace
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
