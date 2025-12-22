# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

#### HTML to PDF Conversion
- **HtmlToPdfConverter**: Simple HTML to PDF conversion with inline CSS support
  - **Supported HTML tags**:
    - Headings: h1, h2, h3, h4, h5, h6
    - Text: p, div, span, b, strong, i, em, u, br
    - Lists: ul, ol, li (with nested list support)
    - Tables: table, tr, td, th
    - Images: img (requires ImageLoader delegate)
  - **Supported CSS properties** (inline styles):
    - `font-size`: 10pt, 12px, 1.5em
    - `color`: Named colors (red, blue, etc.) or hex (#FF0000, #F00)
    - `text-align`: left, center, right
    - `font-weight`: bold, normal, or numeric (>=600 = bold)
    - `font-style`: italic, normal
    - `text-decoration`: underline
    - `width`, `height`: For images (px, pt)
  - **List Features**:
    - Unordered lists with depth-based bullet styles (•, ◦, ▪)
    - Ordered lists with automatic numbering (1., 2., 3., ...)
    - Nested lists support (unlimited depth)
    - Mixed list types (ul within ol, ol within ul)
    - 20pt indentation per nesting level
  - **Image Features**:
    - Custom image loader via `ImageLoaderFunc` delegate
    - Requires user to provide image decoder (e.g., SkiaSharp)
    - Supports width/height attributes and style properties
    - Automatic aspect ratio maintenance
    - Text alignment support (left, center, right)
    - Demo includes SkiaSharp-based image loader example
  - **Table Features**:
    - Basic table structure with headers (th) and data cells (td)
    - Automatic column width calculation (equal distribution)
    - Cell borders with customizable width and color
    - Cell padding for content spacing
    - Bold text for header cells (th tags)
    - Support for multiple rows and columns
    - Mixed content support (tables with text, lists, etc.)
  - **Extension methods**:
    - `CreatePageFromHtml(html)` - Create page from HTML with default A4 size
    - `CreatePageFromHtml(html, marginLeft, marginRight, marginTop, marginBottom, pageWidth, pageHeight)` - With custom margins
  - **Features**:
    - HTML entity decoding (`&lt;`, `&gt;`, `&amp;`, etc.)
    - Nested tag support (e.g., `<b><i>text</i></b>`)
    - Comment stripping (`<!-- comments -->`)
    - Font caching for performance
  - **Limitations**: No external CSS, colspan/rowspan (basic support), or automatic page breaks
  - Examples:
    ```csharp
    // Simple HTML
    using var document = PdfDocument.CreateNew();
    string html = @"
        <h1 style='color: #0066CC; text-align: center;'>Welcome</h1>
        <p>Convert <b>HTML</b> to PDF with ease!</p>
    ";
    document.CreatePageFromHtml(html);
    document.SaveToFile("from-html.pdf");

    // Lists
    string htmlWithLists = @"
        <h1>Task List</h1>
        <ol>
            <li>Preparation
                <ul>
                    <li>Gather requirements</li>
                    <li>Design solution</li>
                </ul>
            </li>
            <li>Implementation</li>
            <li>Testing</li>
        </ol>
    ";

    // Images (requires ImageLoader)
    using var converter = new HtmlToPdfConverter(document);
    converter.ImageLoaderFunc = (src) => {
        // Load and decode image using your preferred library (e.g., SkiaSharp)
        // Return (bgraData, width, height) or null
        return LoadImageToBGRA(src);
    };
    string htmlWithImages = @"
        <h1>Report with Image</h1>
        <div style='text-align: center;'>
            <img src='chart.png' width='400' />
        </div>
    ";

    // Tables
    string htmlWithTables = @"
        <h1>Product Catalog</h1>
        <table>
            <tr>
                <th>Product</th>
                <th>Price</th>
                <th>Stock</th>
            </tr>
            <tr>
                <td>Widget A</td>
                <td>$19.99</td>
                <td>50</td>
            </tr>
            <tr>
                <td>Widget B</td>
                <td>$29.99</td>
                <td>30</td>
            </tr>
        </table>
    ";
    document.CreatePageFromHtml(htmlWithTables);
    ```

#### Enhanced Fluent API for Content Creation
- **PdfColor Helper Class**: Comprehensive color management utilities
  - 40+ predefined colors: basic (Black, White, Red, etc.), extended (Orange, Purple, Pink, etc.), shades (DarkRed, LightBlue, etc.), and highlights
  - `FromArgb(r, g, b, a)` - Create color from ARGB components
  - `FromRgb(r, g, b, opacity)` - Create color from RGB with opacity percentage
  - `FromHex(hex, opacity)` - Create color from hex string (e.g., "#FF0000")
  - `WithOpacity(color, opacity)` - Adjust opacity of existing color
  - Example: `PdfColor.Red`, `PdfColor.FromHex("#FF6B6B")`, `PdfColor.WithOpacity(PdfColor.Blue, 0.5)`

- **PdfFontSize Constants**: 15+ predefined font sizes for consistent typography
  - VerySmall (6pt), Small (8pt), Normal (10pt), Default (12pt)
  - Heading sizes: Heading4 (18pt), Heading3 (20pt), Heading2 (24pt), Heading1 (28pt)
  - Display sizes: Title (48pt), LargeTitle (60pt), Giant (72pt)
  - Example: `PdfFontSize.Heading1`, `PdfFontSize.Normal`

- **PdfContentEditor Enhanced Methods**:
  - **Configuration Methods** (set defaults for subsequent operations):
    - `WithFont(font)` - Set default font
    - `WithFontSize(fontSize)` - Set default font size
    - `WithTextColor(color)` - Set default text color
    - `WithStrokeColor(color)` - Set default stroke color for shapes
    - `WithFillColor(color)` - Set default fill color for shapes
    - `WithLineWidth(width)` - Set default line width
  - **Simplified Text Method**: `Text(text, x, y)` - 3-parameter overload using default font and size
  - **Shape Drawing Methods**:
    - `Line(x1, y1, x2, y2, color, width)` - Draw straight lines
    - `Circle(centerX, centerY, radius, strokeColor, fillColor)` - Draw circles
    - `Ellipse(bounds, strokeColor, fillColor)` - Draw ellipses with Bezier curves
    - `Rectangle(bounds)` - Rectangle overload using default colors
  - **Fluent Chaining**: All methods return `this` for method chaining
  - Example:
    ```csharp
    editor
        .WithFont(font)
        .WithFontSize(PdfFontSize.Heading1)
        .WithTextColor(PdfColor.DarkBlue)
        .Text("Title", 50, 750)
        .WithStrokeColor(PdfColor.Red)
        .WithFillColor(PdfColor.WithOpacity(PdfColor.Red, 0.3))
        .Rectangle(new PdfRectangle(50, 680, 100, 50))
        .Line(50, 650, 250, 650, PdfColor.Black)
        .Circle(100, 600, 30, PdfColor.Blue, PdfColor.WithOpacity(PdfColor.Blue, 0.5))
        .Commit();
    ```

- **QuestPDF-Style Fluent Table API**: Modern table building with discoverable, chainable API
  - **PdfTableBuilder**: Fluent table construction with method chaining
  - **Column Configuration**:
    - `Columns(cols => cols.Add(width))` - Fixed width columns
    - `Columns(cols => cols.Add())` - Auto-width columns (equal distribution)
    - Mix fixed and auto-width columns for flexible layouts
  - **Row Configuration**:
    - `Header(header => header.Cell("text"))` - Define header row with cells
    - `Row(row => row.Cell("text"))` - Add data rows with cells
    - Chain multiple `.Cell()` calls for multi-column rows
  - **Styling Options**:
    - `CellPadding(points)` - Set cell padding (default: 5pt)
    - `BorderWidth(points)` - Set border width (default: 1pt)
    - `BorderColor(color)` - Set border color (default: Black)
    - `HeaderBackgroundColor(color)` - Set header background (default: Transparent)
    - `HeaderTextColor(color)` - Set header text color (default: Black)
    - `HeaderFontSize(size)` - Set header font size (default: 12pt)
    - `HeaderBold(bool)` - Enable/disable bold headers (default: true)
  - **Features**:
    - Automatic column width calculation based on available space
    - Header rows with customizable styling
    - Cell borders and padding
    - Background colors for headers
    - Bold header text support
    - Fluent method chaining for clean, readable code
  - Example:
    ```csharp
    editor.BeginTable()
        .Columns(cols => cols
            .Add(150)   // Fixed: 150pt
            .Add()      // Auto width
            .Add(120))  // Fixed: 120pt
        .HeaderBackgroundColor(PdfColor.WithOpacity(PdfColor.Blue, 0.2))
        .HeaderTextColor(PdfColor.DarkBlue)
        .CellPadding(8)
        .Header(header => header
            .Cell("Name")
            .Cell("Position")
            .Cell("Department"))
        .Row(row => row
            .Cell("John Doe")
            .Cell("Senior Developer")
            .Cell("Engineering"))
        .Row(row => row
            .Cell("Jane Smith")
            .Cell("Product Manager")
            .Cell("Product"))
        .EndTable();
    ```

### Changed
- Multi-targeted `PDFiumZ` to support `net10.0`, `net9.0`, `net8.0`, `netstandard2.1`, and `netstandard2.0`

### Fixed
- `netstandard2.x` build compatibility (`Span<T>`, `IsExternalInit`, and ink annotation interop)
- Math.Clamp compatibility for .NET Standard 2.0 (replaced with Math.Min/Max)

## [145.1.0] - 2025-12-21

### Added

#### Document Creation & Manipulation (Round 1)
- **Create PDF from Scratch**: New `PdfDocument.CreateNew()` method to create empty PDF documents
- **Create Blank Pages**: New `CreatePage(width, height)` method to add blank pages with custom dimensions
- **PDF Merging**: New `PdfDocument.Merge()` static method to combine multiple PDF documents
  - Supports merging 2 or more documents in a single operation
  - Preserves all page content, annotations, and metadata
  - Example: `var merged = PdfDocument.Merge("doc1.pdf", "doc2.pdf", "doc3.pdf");`
- **PDF Splitting**: New `Split(startIndex, pageCount)` method to extract page ranges into new documents
  - Extract specific page ranges from large documents
  - Creates independent documents with selected pages
  - Example: `var split = document.Split(0, 10); // First 10 pages`
- **Text Watermarks**: New `AddTextWatermark()` method with comprehensive customization:
  - Position options: Center, TopLeft, TopRight, BottomLeft, BottomRight, TopCenter, BottomCenter, LeftCenter, RightCenter
  - Opacity control: 0.0 (transparent) to 1.0 (opaque)
  - Rotation angle in degrees (e.g., 45° for diagonal watermarks)
  - Font size customization (default: 48 points)
  - Color customization in ARGB format
  - Example: `document.AddTextWatermark("CONFIDENTIAL", WatermarkPosition.Center, new WatermarkOptions { Opacity = 0.3, Rotation = 45 });`

#### Extended Annotation Support (Round 2)
- **Ink Annotations**: `PdfInkAnnotation.Create()` for freehand drawing
  - Support for multiple strokes (paths)
  - Custom ink color and width
  - Example: `PdfInkAnnotation.Create(page, paths, 0xFF0000FF, 2.0);`
- **FreeText Annotations**: `PdfFreeTextAnnotation.Create()` for text boxes
  - Direct text input on page
  - Custom font size and color
  - Example: `PdfFreeTextAnnotation.Create(page, rect, "Hello World", 0xFF000000, 12);`
- **Rectangle Annotations**: `PdfSquareAnnotation.Create()` with borders and fills
  - Separate border and fill colors
  - Border width customization
  - Transparent fill support with alpha channel
  - Example: `PdfSquareAnnotation.Create(page, bounds, borderColor: 0xFFFF0000, fillColor: 0x400000FF, borderWidth: 2.0);`
- **Circle Annotations**: `PdfCircleAnnotation.Create()` with borders and fills
  - Same customization options as rectangles
  - Perfect for highlighting circular areas
  - Example: `PdfCircleAnnotation.Create(page, bounds, borderColor: 0xFF00FF00, fillColor: 0x4000FF00);`
- **Underline Annotations**: `PdfUnderlineAnnotation.Create()` for text underlining
  - Custom color support
  - Precise text range specification with quad points
  - Example: `PdfUnderlineAnnotation.Create(page, bounds, 0xFFFF0000);`
- **StrikeOut Annotations**: `PdfStrikeOutAnnotation.Create()` for text strikethrough
  - Similar to underline with different visual style
  - Example: `PdfStrikeOutAnnotation.Create(page, bounds, 0xFF0000FF);`

#### Page Operations
- **Page Rotation API**: Three methods for rotating pages with `PdfRotation` enum (Rotate0, Rotate90, Rotate180, Rotate270):
  - `RotatePage(pageIndex, rotation)` - Rotate single page
  - `RotatePages(rotation, params int[] pageIndices)` - Rotate specific pages
  - `RotateAllPages(rotation)` - Rotate all pages at once
  - Also supports setting rotation via `page.Rotation` property
  - Examples:
    ```csharp
    document.RotatePages(PdfRotation.Rotate90, 0, 2, 4);  // Rotate pages 0, 2, 4
    document.RotateAllPages(PdfRotation.Rotate180);       // Rotate all pages
    page.Rotation = PdfRotation.Rotate270;                // Rotate single page
    ```

#### Security & Permissions
- **Security Information Reading**: New `PdfDocument.Security` property providing comprehensive security analysis:
  - `IsEncrypted` - Detect if document is password-protected
  - `EncryptionMethod` - Identify encryption algorithm:
    - "None" for unencrypted documents
    - "40-bit RC4" (Revision 2)
    - "128-bit RC4" (Revision 3)
    - "128-bit AES" (Revision 4)
    - "256-bit AES" (Revision 5)
    - "256-bit AES (PDF 2.0)" (Revision 6)
  - `SecurityHandlerRevision` - Get security handler version
  - `Permissions` and `UserPermissions` - Raw permission flags
  - Convenience properties with smart checking (unencrypted = all allowed):
    - `CanPrint`, `CanPrintHighQuality` - Print permissions
    - `CanModifyContents` - Modify document content
    - `CanCopyContent` - Copy text and graphics
    - `CanModifyAnnotations` - Add/modify annotations
    - `CanFillForms` - Fill form fields
    - `CanExtractForAccessibility` - Extract for accessibility
    - `CanAssembleDocument` - Assemble document (insert/delete pages)
  - Example:
    ```csharp
    var security = document.Security;
    if (security.IsEncrypted) {
        Console.WriteLine($"Encryption: {security.EncryptionMethod}");
    }
    Console.WriteLine($"Can print: {security.CanPrint}");
    ```
- **New `PdfPermissions` enum**: Standard PDF permission flags per PDF Reference 1.7, Table 3.20
  - Print (Bit 3), ModifyContents (Bit 4), CopyContent (Bit 5)
  - ModifyAnnotations (Bit 6), FillForms (Bit 9), ExtractAccessibility (Bit 10)
  - AssembleDocument (Bit 11), PrintHighQuality (Bit 12)
  - Supports bitwise operations with [Flags] attribute
- **New `PdfSecurityInfo` class**: Sealed class with internal constructor for controlled access

**Important**: PDFiumZ can read PDF security information but does not support encrypting or password-protecting PDFs. This is a PDFium library limitation - the underlying library only provides read access to security features.

#### Performance & Quality Assurance
- **Comprehensive Performance Benchmarks**: 23 benchmarks using BenchmarkDotNet v0.15.8
  - **Document Loading**: Small PDF (1 page), Medium PDF (10 pages)
  - **Page Operations**: Single page access, batch page retrieval, page property access
  - **Rendering Performance**: 72 DPI, 150 DPI, 300 DPI rendering tests
  - **Text Operations**: Text extraction, text search
  - **Document Manipulation**: Merge 3 documents, split document, rotate all pages
  - **Save Operations**: Save small document, save medium document
  - **Content Creation**: Add text with font, add watermark
  - **Metadata Access**: Access document metadata, access security info
  - **Real-World Workflows**: Load → Render → Save, Load → Modify → Save
  - **Memory Diagnostics**: Gen0/Gen1/Gen2 GC statistics and allocation tracking
  - **Performance Ranking**: Automatic ranking by execution time
- **Performance Analysis Report (PERFORMANCE.md)**: Comprehensive 249-line analysis including:
  - Executive summary with key findings
  - Detailed benchmark results by category
  - Performance rankings (fastest to slowest operations)
  - Memory efficiency analysis
  - Optimization opportunities identification:
    - High Priority: Batch page access optimization, Page load caching
    - Medium Priority: Rendering pipeline optimization, Memory pooling
    - Low Priority: Async I/O considerations
  - Best practices for library users (DPI selection, document reuse, async operations, batching)
  - Best practices for library developers (caching, batch APIs, memory pooling, parallel rendering)
  - Performance characteristics summary by category
  - Key insights:
    - Document loading: 52-65 μs (extremely fast)
    - Page creation: 4.682 μs (fastest operation)
    - Rendering scales quadratically with DPI
    - Memory footprint: <1KB for most operations
- **Benchmark Documentation**: Detailed benchmarking guide (PDFiumZ.Benchmarks/README.md)
  - Instructions for running benchmarks
  - Benchmark category explanations
  - Performance tips and optimization strategies
  - Contributing guidelines for new benchmarks

#### Testing
- **Comprehensive Unit Test Coverage**: 37 unit tests across multiple test classes
  - **Document Creation Tests**: CreateNew, CreatePage validation
  - **Watermark Tests**: Position, opacity, rotation, font customization
  - **Merge & Split Tests**: Multi-document merging, page range extraction
  - **Extended Annotation Tests**: Ink, FreeText, Square, Circle, Underline, StrikeOut annotations
  - **Page Rotation Tests**: Single page, multiple pages, all pages rotation
  - **Security Tests**: 8 tests covering:
    - Encryption detection for unencrypted documents
    - Security handler revision retrieval
    - Permission flag reading for unencrypted documents
    - Convenience property validation (CanPrint, CanModify, etc.)
    - Smart permission checking (unencrypted = all allowed)
  - All tests passing with proper resource cleanup
  - Uses xUnit framework with IDisposable pattern

### Changed
- **Project renamed to PDFiumZ** - Package ID and assembly name changed from PDFiumZ to PDFiumZ
- Namespaces remain as `PDFiumZ` and `PDFiumZ.HighLevel` for API compatibility
- Updated package description and metadata
- Reorganized documentation for clarity
- Improved annotation API consistency across all annotation types
- Enhanced error handling for invalid rotation angles
- Optimized memory usage in batch operations

### Documentation
- **README.md**: Added comprehensive examples for all v145.1.0 features:
  - **Create PDF from Scratch** section with CreateNew and CreatePage examples
  - **Merge and Split PDFs** section with multi-document operations
  - **Add Watermarks** section with full customization examples
  - **Rotate Pages** section showing all three rotation methods
  - **Check PDF Security and Permissions** section with complete security inspection
  - **Performance Benchmarks** section with benchmark running instructions
  - Updated Features list with bold markers for new v145.1.0 features
  - All code examples tested and verified
- **PERFORMANCE.md**: New comprehensive 249-line performance analysis report
  - Executive summary with key performance metrics
  - Complete benchmark results organized by category
  - Performance rankings from fastest to slowest operations
  - Memory efficiency analysis with allocation statistics
  - High/Medium/Low priority optimization opportunities
  - Best practices for library users and developers
  - Real-world workflow performance characteristics
- **ROADMAP.md**: Updated to reflect accurate project status
  - Current completion: 92% (Round 1: 100%, Round 2: 85%)
  - Detailed breakdown of completed features by phase
  - Updated security feature status (read-only implementation)
  - Updated performance optimization status (complete with 23 benchmarks)
  - Updated documentation status (comprehensive guides completed)
  - Clear next steps: v145.1.0 finalization and release preparation
- **PDFiumZ.Benchmarks/README.md**: New detailed benchmarking guide
  - Prerequisites and setup instructions
  - Running benchmarks (all tests and filtered tests)
  - Benchmark categories explained (9 categories, 23 tests)
  - Understanding BenchmarkDotNet results
  - Performance tips based on benchmark findings
  - Contributing guidelines for new benchmarks
  - Continuous performance monitoring recommendations

### Fixed
- Fixed permission checking logic for unencrypted documents
  - Now correctly returns all permissions as allowed for unencrypted PDFs
  - Smart checking: `!IsEncrypted || Permissions.HasFlag(...)` pattern
  - Previously incorrectly checked flags on 0 value
- Improved security information accuracy for documents with no encryption

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

- **Async API Support** - Non-blocking async operations with cancellation support
  - `PdfDocument` async methods:
    - `OpenAsync(filePath, password?, cancellationToken)` - Async document loading from file
    - `OpenFromMemoryAsync(data, password?, cancellationToken)` - Async loading from memory
    - `SaveToFileAsync(filePath, cancellationToken)` - Async document saving
  - `PdfPage` async methods:
    - `RenderToImageAsync(cancellationToken)` - Async rendering with default options
    - `RenderToImageAsync(options, cancellationToken)` - Async rendering with custom options
  - All async methods support `CancellationToken` for operation cancellation
  - Designed for responsive UI applications and high-throughput scenarios
  - Compatible with Task-based Asynchronous Pattern (TAP)

- **Batch Operations** - Efficient multi-page processing
  - `PdfDocument.GetPages(startIndex, count)` - Retrieve consecutive page range with lazy loading
  - `PdfDocument.DeletePages(params int[])` - Delete multiple pages by indices
    - Automatic descending order sorting to prevent index shift issues
    - Duplicate index detection and removal
  - `PdfDocument.DeletePages(startIndex, count)` - Delete consecutive page range
  - Optimized for processing large documents efficiently
  - Memory-efficient lazy loading for page retrieval

- **Demo Application Updates**
  - Added comprehensive examples for all high-level API features
  - Demonstrates rendering, text extraction, and page manipulation
  - Added annotation management demo with all annotation types (highlight, text, stamps)
  - Shows annotation creation, reading, filtering, and removal operations
  - Added content creation demo with text and shapes
  - Demonstrates font loading and management
  - Added async operations demo with timing and cancellation
  - Added batch operations demo with page retrieval and deletion
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
