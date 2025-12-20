# PDFiumZ High-Level API Guide

## Table of Contents
- [Getting Started](#getting-started)
- [Development Plan](#development-plan)
- [Document Management](#document-management)
- [Page Operations](#page-operations)
- [Rendering](#rendering)
- [Text Extraction](#text-extraction)
- [Document Metadata](#document-metadata)
- [Bookmarks](#bookmarks)
- [Page Manipulation](#page-manipulation)
- [Form Fields](#form-fields)
- [Saving Documents](#saving-documents)
- [Best Practices](#best-practices)

## Development Plan

### 目标
- 完善高层 API 的一致性（文件/内存/流三种输入输出对齐）
- 强化资源生命周期与线程安全边界（尽量“用错就报错”，不 silent failure）
- 把性能/内存开销控制在“高层封装可接受”的范围（减少不必要拷贝与分配）
- 补齐自动化验证（单测 + CI 对齐目标框架）

### 里程碑（按优先级）

#### P0：正确性与易用性（1–2 周）
- `OpenFromMemory` 生命周期：确保内存数据在文档关闭前始终有效（避免 GCHandle 过早释放导致潜在崩溃/读错）
- `SaveToStream(Stream)`：补齐与 `SaveToFile` 对称的保存方式，支持写入任意可写流
- 初始化体验：提供 `IDisposable` 作用域式初始化封装（例如 `using var _ = PdfiumLibrary.EnterScope();`），避免漏调用 `Shutdown`
- 错误语义：把“需要密码/密码错误”等错误码映射为 `PdfPasswordException`（而不是泛化的加载失败）

#### P1：API 能力扩展（2–4 周）
- `OpenFromStream(Stream)`：支持从流加载（优先考虑零额外拷贝方案，其次退化为一次性读入）
- 保存增强：支持保存到 byte[]（或 `MemoryStream` 便捷方法）以适配 Web/云函数场景
- 文本能力：扩展 `ExtractText` 的范围/布局选项（例如指定区域、行/词边界、更多搜索选项）
- 页面能力：补齐常用查询（页面裁剪框、旋转后尺寸、坐标转换工具方法）

#### P2：性能与内存优化（4–6 周）
- 保存写入：`PdfFileWriter` 回调写入使用 `ArrayPool<byte>`/分段写入以减少频繁分配
- 文本读取：对小字符串场景用 `stackalloc`/复用缓冲策略减少 GC 压力（保持 API 返回 `string` 不变）
- 渲染路径：为高频缩略图/预览场景提供复用 bitmap 的渲染入口（减少反复创建/销毁 bitmap）
- 图片提取：批量提取时减少异常控制流（避免用 try/catch 做常态分支）

#### P3：质量与工程化（持续）
- 新增 `PDFiumZ.Tests`：覆盖打开/保存/渲染/文本/书签/表单字段的关键路径与异常路径
- CI 对齐：将 GitHub Actions 使用的 .NET SDK 版本与项目目标框架保持一致，并增加 `dotnet test`
- 基准：为渲染与文本提取提供可重复的基准项目（用于回归比较）

### 验收标准（每个里程碑通用）
- API 行为可预测：参数校验明确、异常类型稳定、资源释放无泄漏
- 性能可量化：关键路径有基准数据，优化前后有对比
- 回归可控：新增能力必须有对应测试覆盖（至少 Happy Path + 典型错误）

## Getting Started

### Installation

```bash
dotnet add package PDFiumZ
dotnet add package PDFiumZ.SkiaSharp  # Optional: for image export
```

### Basic Setup

```csharp
using PDFiumZ;
using PDFiumZ.HighLevel;

// Initialize library (call once at application start)
PdfiumLibrary.Initialize();

try
{
    // Your PDF operations here
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

## Document Management

### Opening Documents

```csharp
// From file path
using var document = PdfDocument.Open("sample.pdf");

// With password
using var encrypted = PdfDocument.Open("protected.pdf", "password");

// From memory (byte array)
byte[] pdfData = File.ReadAllBytes("sample.pdf");
using var document = PdfDocument.OpenFromMemory(pdfData);
```

### Document Properties

```csharp
using var document = PdfDocument.Open("sample.pdf");

Console.WriteLine($"Pages: {document.PageCount}");
Console.WriteLine($"File version: {document.FileVersion}");  // e.g., 17 for PDF 1.7
Console.WriteLine($"Permissions: {document.Permissions}");

// Check if file was loaded from disk
if (document.FilePath != null)
{
    Console.WriteLine($"Loaded from: {document.FilePath}");
}
```

## Page Operations

### Accessing Pages

```csharp
using var document = PdfDocument.Open("sample.pdf");

// Get single page (zero-based index)
using var page = document.GetPage(0);

Console.WriteLine($"Width: {page.Width} points");    // 1 point = 1/72 inch
Console.WriteLine($"Height: {page.Height} points");
Console.WriteLine($"Rotation: {page.Rotation}°");    // 0, 90, 180, or 270

// Get page size as tuple
var (width, height) = page.Size;
```

### Iterating Pages

```csharp
using var document = PdfDocument.Open("sample.pdf");

for (int i = 0; i < document.PageCount; i++)
{
    using var page = document.GetPage(i);
    Console.WriteLine($"Page {i + 1}: {page.Width} x {page.Height}");
}
```

## Rendering

### Basic Rendering

```csharp
using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);

// Render with default settings (72 DPI, white background)
using var image = page.RenderToImage();

Console.WriteLine($"Rendered: {image.Width} x {image.Height} pixels");
Console.WriteLine($"Format: {image.Format}");  // BGRA or BGRx
Console.WriteLine($"Stride: {image.Stride} bytes per row");
```

### Advanced Rendering Options

```csharp
// Fluent API for rendering configuration
var options = RenderOptions.Default
    .WithDpi(150)                                    // 150 DPI (2.08x scale)
    .WithScale(2.0)                                  // Or use custom scale
    .WithTransparency()                              // Transparent background
    .WithBackgroundColor(0xFFFFFFFF)                 // Or custom color (ARGB)
    .AddFlags(RenderFlags.OptimizeTextForLcd)       // LCD text optimization
    .AddFlags(RenderFlags.RenderAnnotations);       // Include annotations

using var image = page.RenderToImage(options);
```

### Available Render Flags

```csharp
RenderFlags.RenderAnnotations        // Render annotations
RenderFlags.OptimizeTextForLcd       // Optimize text for LCD displays
RenderFlags.NoNativeText             // Don't use native text rendering
RenderFlags.Grayscale                // Render in grayscale
RenderFlags.LimitedImageCacheSize    // Limit image cache size
RenderFlags.ForceHalftone            // Force halftone for image stretching
RenderFlags.PrintingMode             // Render for printing
RenderFlags.RenderFormFill           // Render form fields
RenderFlags.NoSmoothText             // Disable text anti-aliasing
RenderFlags.NoSmoothImage            // Disable image smoothing
RenderFlags.NoSmoothPath             // Disable path anti-aliasing
```

### Working with Image Data

```csharp
using var image = page.RenderToImage();

// Get raw pixel buffer as IntPtr
IntPtr buffer = image.Buffer;

// Get pixel data as Span<byte> (zero-copy)
Span<byte> pixels = image.GetPixelSpan();

// Copy to byte array
byte[] pixelData = image.ToByteArray();

// Calculate total buffer size
int bufferSize = image.Stride * image.Height;
```

### Saving Images (with SkiaSharp extension)

```csharp
using PDFiumZ.SkiaSharp;

using var image = page.RenderToImage();

// Save as PNG
image.SaveAsSkiaPng("output.png", quality: 100);

// Save as JPEG
image.SaveAsSkiaJpeg("output.jpg", quality: 90);

// Convert to SKBitmap for further processing
using var skBitmap = image.ToSkiaBitmap();
// ... use SkiaSharp APIs
```

## Text Extraction

### Extract Page Text

```csharp
using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);

string text = page.ExtractText();
Console.WriteLine($"Extracted {text.Length} characters");
Console.WriteLine(text);
```

### Extract Text from All Pages

```csharp
using var document = PdfDocument.Open("sample.pdf");

for (int i = 0; i < document.PageCount; i++)
{
    using var page = document.GetPage(i);
    string text = page.ExtractText();
    Console.WriteLine($"--- Page {i + 1} ---");
    Console.WriteLine(text);
}
```

## Document Metadata

### Accessing Metadata

```csharp
using var document = PdfDocument.Open("sample.pdf");

// Access metadata (cached after first access)
var meta = document.Metadata;

Console.WriteLine($"Title: {meta.Title}");
Console.WriteLine($"Author: {meta.Author}");
Console.WriteLine($"Subject: {meta.Subject}");
Console.WriteLine($"Keywords: {meta.Keywords}");
Console.WriteLine($"Creator: {meta.Creator}");           // Application that created original document
Console.WriteLine($"Producer: {meta.Producer}");         // PDF producer application
Console.WriteLine($"Creation Date: {meta.CreationDate ?? "Not available"}");
Console.WriteLine($"Modification Date: {meta.ModificationDate ?? "Not available"}");
```

### Understanding Metadata Properties

**Basic Document Information:**
- `Title` - Document title
- `Author` - Document author
- `Subject` - Document subject or description
- `Keywords` - Keywords associated with document

**Application Information:**
- `Creator` - Name of application that created the original document (e.g., "Microsoft Word")
- `Producer` - Name of application that produced the PDF (e.g., "Acrobat Distiller")

**Timestamps:**
- `CreationDate` - When document was created (PDF date format: "D:YYYYMMDDHHmmSSOHH'mm'")
- `ModificationDate` - When document was last modified (PDF date format)

### Date Format

PDF dates use a specific format string:
```
D:YYYYMMDDHHmmSSOHH'mm'

Example: D:20231225120000+08'00'
  Year:   2023
  Month:  12 (December)
  Day:    25
  Hour:   12
  Minute: 00
  Second: 00
  TZ:     +08'00' (UTC+8)
```

### Handling Missing Metadata

```csharp
using var document = PdfDocument.Open("sample.pdf");
var meta = document.Metadata;

// Check for empty strings
if (string.IsNullOrEmpty(meta.Title))
{
    Console.WriteLine("No title available");
}

// Dates may be null if not present
if (meta.CreationDate != null)
{
    Console.WriteLine($"Created: {meta.CreationDate}");
}
else
{
    Console.WriteLine("Creation date not available");
}
```

### Practical Examples

**Generate Document Info Report:**
```csharp
using var document = PdfDocument.Open("sample.pdf");
var meta = document.Metadata;

Console.WriteLine("=== Document Information ===");
Console.WriteLine($"File: {document.FilePath}");
Console.WriteLine($"Pages: {document.PageCount}");
Console.WriteLine($"PDF Version: 1.{document.FileVersion / 10}");
Console.WriteLine();

Console.WriteLine("=== Metadata ===");
Console.WriteLine($"Title: {meta.Title}");
Console.WriteLine($"Author: {meta.Author}");
Console.WriteLine($"Subject: {meta.Subject}");
Console.WriteLine($"Keywords: {meta.Keywords}");
Console.WriteLine();

Console.WriteLine("=== Technical Information ===");
Console.WriteLine($"Creator: {meta.Creator}");
Console.WriteLine($"Producer: {meta.Producer}");
Console.WriteLine($"Created: {meta.CreationDate ?? "Unknown"}");
Console.WriteLine($"Modified: {meta.ModificationDate ?? "Unknown"}");
```

**Search PDFs by Metadata:**
```csharp
string[] pdfFiles = Directory.GetFiles("./documents", "*.pdf");
var results = new List<(string File, string Author, string Title)>();

PdfiumLibrary.Initialize();
try
{
    foreach (var file in pdfFiles)
    {
        using var doc = PdfDocument.Open(file);
        var meta = doc.Metadata;

        // Search by author
        if (meta.Author.Contains("John Doe", StringComparison.OrdinalIgnoreCase))
        {
            results.Add((file, meta.Author, meta.Title));
        }
    }
}
finally
{
    PdfiumLibrary.Shutdown();
}

foreach (var (file, author, title) in results)
{
    Console.WriteLine($"{file}: {title} by {author}");
}
```

**Extract Metadata to JSON:**
```csharp
using System.Text.Json;

using var document = PdfDocument.Open("sample.pdf");
var meta = document.Metadata;

var json = JsonSerializer.Serialize(new
{
    title = meta.Title,
    author = meta.Author,
    subject = meta.Subject,
    keywords = meta.Keywords,
    creator = meta.Creator,
    producer = meta.Producer,
    creationDate = meta.CreationDate,
    modificationDate = meta.ModificationDate,
    pageCount = document.PageCount,
    pdfVersion = $"1.{document.FileVersion / 10}"
}, new JsonSerializerOptions { WriteIndented = true });

File.WriteAllText("metadata.json", json);
```

## Bookmarks

### Listing Bookmarks

```csharp
using var document = PdfDocument.Open("sample.pdf");

// Get first bookmark
var firstBookmark = document.GetFirstBookmark();
if (firstBookmark != null)
{
    Console.WriteLine($"First: {firstBookmark.Title}");
}

// Enumerate all root-level bookmarks
foreach (var bookmark in document.GetBookmarks())
{
    Console.WriteLine(bookmark.Title);
}
```

### Navigating Bookmark Tree

```csharp
foreach (var bookmark in document.GetBookmarks())
{
    // Get bookmark properties
    Console.WriteLine($"Title: {bookmark.Title}");
    Console.WriteLine($"Child count: {bookmark.ChildCount}");

    // Get destination
    var dest = bookmark.Destination;
    if (dest != null && dest.HasValidPage)
    {
        Console.WriteLine($"Points to page: {dest.PageIndex}");
    }

    // Navigate to first child
    var child = bookmark.GetFirstChild();
    if (child != null)
    {
        Console.WriteLine($"  First child: {child.Title}");
    }

    // Enumerate all children
    foreach (var childBookmark in bookmark.GetChildren())
    {
        Console.WriteLine($"  - {childBookmark.Title}");
    }

    // Recursively get all descendants
    foreach (var descendant in bookmark.GetAllDescendants())
    {
        Console.WriteLine($"  Descendant: {descendant.Title}");
    }
}
```

### Finding Bookmarks

```csharp
// Search by exact title
var bookmark = document.FindBookmark("Chapter 1");
if (bookmark != null)
{
    Console.WriteLine($"Found: {bookmark.Title}");
    var dest = bookmark.Destination;
    if (dest?.HasValidPage == true)
    {
        // Jump to bookmark destination
        using var page = document.GetPage(dest.PageIndex);
        // ... render or process page
    }
}
```

## Page Manipulation

### Inserting Pages

```csharp
using var document = PdfDocument.Open("sample.pdf");

// Insert blank A4 page at beginning (595 x 842 points)
document.InsertBlankPage(0, 595, 842);

// Insert Letter size page (612 x 792 points)
document.InsertBlankPage(1, 612, 792);

// Insert at end
document.InsertBlankPage(document.PageCount, 595, 842);
```

### Deleting Pages

```csharp
// Delete specific page (zero-based index)
document.DeletePage(0);

// Delete last page
document.DeletePage(document.PageCount - 1);

// Delete multiple pages
for (int i = 2; i >= 0; i--)  // Delete in reverse to maintain indices
{
    document.DeletePage(i);
}
```

### Moving Pages

```csharp
// Move single page to end
document.MovePages(new[] { 0 }, document.PageCount - 1);

// Move multiple pages to position 2
document.MovePages(new[] { 5, 6, 7 }, 2);

// Swap pages
document.MovePages(new[] { 0 }, 2);  // Move page 0 to position 2
```

### Importing Pages

```csharp
using var source = PdfDocument.Open("source.pdf");
using var target = PdfDocument.Open("target.pdf");

// Import all pages to end
target.ImportPages(source);

// Import specific pages by range (1-based page numbers)
target.ImportPages(source, "1,3,5-7");

// Import to specific position
target.ImportPages(source, "1-3", insertAtIndex: 0);

// Import by exact indices (zero-based)
target.ImportPagesAt(source, new[] { 0, 2, 4 }, insertAtIndex: 1);
```

### Common Page Manipulation Scenarios

```csharp
// Duplicate a page
using var document = PdfDocument.Open("sample.pdf");
using var temp = PdfDocument.OpenFromMemory(File.ReadAllBytes("sample.pdf"));
document.ImportPagesAt(temp, new[] { 0 }, insertAtIndex: 1);

// Reverse page order
var pageCount = document.PageCount;
for (int i = 0; i < pageCount - 1; i++)
{
    document.MovePages(new[] { pageCount - 1 }, i);
}

// Remove every other page
for (int i = document.PageCount - 1; i >= 0; i -= 2)
{
    document.DeletePage(i);
}
```

## Form Fields

### Reading Form Field Information

```csharp
using var document = PdfDocument.Open("form.pdf");
using var page = document.GetPage(0);

// Get form field count
var fieldCount = page.GetFormFieldCount();
Console.WriteLine($"Found {fieldCount} form field(s)");

// Iterate through all form fields
foreach (var field in page.GetFormFields())
{
    using (field)
    {
        Console.WriteLine($"Name: {field.Name}");
        Console.WriteLine($"Type: {field.FieldType}");
        Console.WriteLine($"Value: {field.Value}");
    }
}
```

### Form Field Types

```csharp
public enum PdfFormFieldType
{
    Unknown = -1,       // Unknown or invalid field
    PushButton = 0,     // Action button
    CheckBox = 1,       // Checkbox (toggle)
    RadioButton = 2,    // Radio button (group selection)
    ComboBox = 3,       // Dropdown list
    ListBox = 4,        // List selection
    TextField = 5,      // Text input (single/multi-line)
    Signature = 6       // Digital signature field
}
```

### Working with Text Fields

```csharp
foreach (var field in page.GetFormFields())
{
    using (field)
    {
        if (field.FieldType == PdfFormFieldType.TextField)
        {
            Console.WriteLine($"Text Field: {field.Name}");
            Console.WriteLine($"Current Value: {field.Value}");

            // Get alternate name (user-friendly label)
            if (!string.IsNullOrEmpty(field.AlternateName))
                Console.WriteLine($"Label: {field.AlternateName}");
        }
    }
}
```

### Working with Checkboxes and Radio Buttons

```csharp
foreach (var field in page.GetFormFields())
{
    using (field)
    {
        if (field.FieldType == PdfFormFieldType.CheckBox)
        {
            Console.WriteLine($"Checkbox: {field.Name}");
            Console.WriteLine($"Checked: {field.IsChecked}");
        }

        if (field.FieldType == PdfFormFieldType.RadioButton)
        {
            Console.WriteLine($"Radio: {field.Name}");
            Console.WriteLine($"Selected: {field.IsChecked}");
        }
    }
}
```

### Working with Combo Boxes and List Boxes

```csharp
foreach (var field in page.GetFormFields())
{
    using (field)
    {
        if (field.FieldType == PdfFormFieldType.ComboBox ||
            field.FieldType == PdfFormFieldType.ListBox)
        {
            Console.WriteLine($"{field.FieldType}: {field.Name}");
            Console.WriteLine($"Current Value: {field.Value}");

            // Get available options
            var options = field.GetAllOptions();
            Console.WriteLine($"Available options: {string.Join(", ", options)}");

            // Check individual option selection (list boxes can have multiple)
            for (int i = 0; i < field.GetOptionCount(); i++)
            {
                var label = field.GetOptionLabel(i);
                var selected = field.IsOptionSelected(i);
                Console.WriteLine($"  {label}: {(selected ? "Selected" : "Not selected")}");
            }
        }
    }
}
```

### Accessing Field Properties

```csharp
using var field = page.GetFormField(0);

// Basic properties
string name = field.Name;                    // Unique field identifier
string alternateName = field.AlternateName;  // User-friendly label
string value = field.Value;                  // Current field value
PdfFormFieldType type = field.FieldType;     // Field type
int index = field.Index;                     // Index in page
int flags = field.Flags;                     // Field flags (read-only, required, etc.)

// State properties
bool isChecked = field.IsChecked;            // For checkboxes/radio buttons

// Dropdown/list properties
int optionCount = field.GetOptionCount();
string[] allOptions = field.GetAllOptions();
```

### Searching for Specific Fields

```csharp
using var document = PdfDocument.Open("form.pdf");

// Search all pages for a specific field
PdfFormField? FindField(string fieldName)
{
    for (int i = 0; i < document.PageCount; i++)
    {
        using var page = document.GetPage(i);
        foreach (var field in page.GetFormFields())
        {
            if (field.Name == fieldName)
                return field;
            field.Dispose();
        }
    }
    return null;
}

// Usage
using var emailField = FindField("email");
if (emailField != null)
{
    Console.WriteLine($"Email value: {emailField.Value}");
}
```

### Extracting All Form Data

```csharp
using var document = PdfDocument.Open("form.pdf");
var formData = new Dictionary<string, string>();

for (int i = 0; i < document.PageCount; i++)
{
    using var page = document.GetPage(i);
    foreach (var field in page.GetFormFields())
    {
        using (field)
        {
            // Store field name and value
            formData[field.Name] = field.Value;

            // For checkboxes, store checked state
            if (field.FieldType == PdfFormFieldType.CheckBox ||
                field.FieldType == PdfFormFieldType.RadioButton)
            {
                formData[field.Name] = field.IsChecked.ToString();
            }
        }
    }
}

// Export to JSON, XML, or database
foreach (var (name, value) in formData)
{
    Console.WriteLine($"{name} = {value}");
}
```

### Form Field Report Generation

```csharp
using var document = PdfDocument.Open("form.pdf");

Console.WriteLine("=== Form Fields Report ===\n");

for (int pageNum = 0; pageNum < document.PageCount; pageNum++)
{
    using var page = document.GetPage(pageNum);
    var fields = page.GetFormFields().ToList();

    if (fields.Count > 0)
    {
        Console.WriteLine($"Page {pageNum + 1}:");

        foreach (var field in fields)
        {
            using (field)
            {
                Console.WriteLine($"\n  Field: {field.Name}");
                Console.WriteLine($"  Type: {field.FieldType}");

                if (!string.IsNullOrEmpty(field.AlternateName))
                    Console.WriteLine($"  Label: {field.AlternateName}");

                if (!string.IsNullOrEmpty(field.Value))
                    Console.WriteLine($"  Value: {field.Value}");

                if (field.FieldType == PdfFormFieldType.CheckBox ||
                    field.FieldType == PdfFormFieldType.RadioButton)
                {
                    Console.WriteLine($"  Checked: {field.IsChecked}");
                }

                if (field.FieldType == PdfFormFieldType.ComboBox ||
                    field.FieldType == PdfFormFieldType.ListBox)
                {
                    var options = field.GetAllOptions();
                    if (options.Length > 0)
                    {
                        Console.WriteLine($"  Options: {string.Join(", ", options)}");
                    }
                }
            }
        }
        Console.WriteLine();
    }
}
```

### Limitations

**Current Implementation:**
- ✅ Read all form field properties
- ✅ Access field values and states
- ✅ Enumerate dropdown/listbox options
- ✅ Check checkbox/radio button states
- ✅ Support for all standard field types

**Not Supported (requires interactive form environment):**
- ❌ Modifying field values
- ❌ Interactive form filling
- ❌ Form submission
- ❌ Field validation

For interactive form filling, use the low-level `fpdf_annot` and `fpdf_formfill` APIs directly.

## Saving Documents

### Basic Save

```csharp
using var document = PdfDocument.Open("sample.pdf");

// Modify document...
document.InsertBlankPage(0, 595, 842);

// Save to file
document.SaveToFile("modified.pdf");
```

### Complete Workflow Example

```csharp
using var source1 = PdfDocument.Open("part1.pdf");
using var source2 = PdfDocument.Open("part2.pdf");
using var combined = PdfDocument.Open("template.pdf");

// Insert blank cover page
combined.InsertBlankPage(0, 595, 842);

// Import pages from multiple sources
combined.ImportPages(source1);
combined.ImportPages(source2);

// Delete unwanted pages
combined.DeletePage(5);

// Save result
combined.SaveToFile("combined.pdf");
```

## Best Practices

### Resource Management

```csharp
// ✅ GOOD: Use 'using' statements
using (var document = PdfDocument.Open("sample.pdf"))
{
    using (var page = document.GetPage(0))
    {
        using (var image = page.RenderToImage())
        {
            // Use image...
        }
    }
}

// ✅ BETTER: Use 'using' declarations (C# 8.0+)
using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);
using var image = page.RenderToImage();
// Automatically disposed at end of scope

// ❌ BAD: Not disposing resources
var document = PdfDocument.Open("sample.pdf");  // Memory leak!
```

### Library Initialization

```csharp
// ✅ GOOD: Initialize once, shutdown once
public class PdfService
{
    static PdfService()
    {
        PdfiumLibrary.Initialize();
    }

    // ... PDF operations
}

// Register cleanup on application shutdown
AppDomain.CurrentDomain.ProcessExit += (s, e) => PdfiumLibrary.Shutdown();

// ❌ BAD: Initializing for every operation
void ProcessPdf(string path)
{
    PdfiumLibrary.Initialize();  // Don't do this repeatedly!
    using var doc = PdfDocument.Open(path);
    // ...
    PdfiumLibrary.Shutdown();  // Don't do this until app exit!
}
```

### Error Handling

```csharp
try
{
    using var document = PdfDocument.Open("sample.pdf", "password");
    // ... operations
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.FileName}");
}
catch (PdfPasswordException ex)
{
    Console.WriteLine("Incorrect password or password required");
}
catch (PdfLoadException ex)
{
    Console.WriteLine($"Failed to load PDF: {ex.Message}");
}
catch (PdfRenderException ex)
{
    Console.WriteLine($"Rendering failed: {ex.Message}");
}
catch (PdfException ex)
{
    Console.WriteLine($"PDF operation failed: {ex.Message}");
}
```

### Performance Tips

```csharp
// ✅ Cache RenderOptions for reuse
var highDpiOptions = RenderOptions.Default.WithDpi(150);

for (int i = 0; i < document.PageCount; i++)
{
    using var page = document.GetPage(i);
    using var image = page.RenderToImage(highDpiOptions);  // Reuse options
    // ...
}

// ✅ Use zero-copy access when possible
using var image = page.RenderToImage();
Span<byte> pixels = image.GetPixelSpan();  // No memory copy
// Process pixels directly...

// ❌ Avoid unnecessary copies
byte[] copy = image.ToByteArray();  // Creates copy - only if needed
```

### Thread Safety

```csharp
// ⚠️ Important: Document/Page objects are NOT thread-safe
// Create separate instances per thread

// ✅ GOOD: One document per thread
Parallel.For(0, fileCount, i =>
{
    using var document = PdfDocument.Open($"file{i}.pdf");
    // Process document...
});

// ❌ BAD: Sharing document across threads
using var document = PdfDocument.Open("sample.pdf");
Parallel.For(0, document.PageCount, i =>
{
    using var page = document.GetPage(i);  // Race condition!
});
```

## API Reference Summary

### PdfiumLibrary (Static)
- `Initialize()` - Initialize PDFium library
- `Shutdown()` - Shutdown PDFium library
- `IsInitialized` - Check initialization status

### PdfDocument
- **Creation**: `Open()`, `OpenFromMemory()`
- **Properties**: `PageCount`, `FileVersion`, `Permissions`, `FilePath`
- **Pages**: `GetPage()`
- **Bookmarks**: `GetFirstBookmark()`, `GetBookmarks()`, `FindBookmark()`
- **Manipulation**: `InsertBlankPage()`, `DeletePage()`, `MovePages()`, `ImportPages()`, `ImportPagesAt()`
- **Saving**: `SaveToFile()`

### PdfPage
- **Properties**: `Index`, `Width`, `Height`, `Rotation`, `Size`
- **Operations**: `RenderToImage()`, `ExtractText()`

### PdfImage
- **Properties**: `Width`, `Height`, `Stride`, `Format`, `Buffer`
- **Methods**: `GetPixelSpan()`, `ToByteArray()`
- **Extensions** (SkiaSharp): `ToSkiaBitmap()`, `SaveAsSkiaPng()`, `SaveAsSkiaJpeg()`

### RenderOptions (Immutable Record)
- **Methods**: `WithDpi()`, `WithScale()`, `WithViewport()`, `WithTransparency()`, `WithBackgroundColor()`, `AddFlags()`
- **Properties**: `Dpi`, `Scale`, `Viewport`, `HasTransparency`, `BackgroundColor`, `Flags`

### PdfBookmark
- **Properties**: `Title`, `ChildCount`, `Destination`
- **Navigation**: `GetFirstChild()`, `GetNextSibling()`, `GetChildren()`, `GetAllDescendants()`

### PdfDestination
- **Properties**: `PageIndex`, `HasValidPage`

## Exception Hierarchy

```
Exception
└── PdfException - Base exception for all PDF operations
    ├── PdfLoadException - Failed to load/open document
    ├── PdfPasswordException - Password required or incorrect
    └── PdfRenderException - Rendering operation failed
```

## Version Compatibility

- **Minimum .NET**: .NET 8.0+
- **Supported Platforms**: Windows (x64, x86), Linux (x64), macOS (x64)
- **PDFium Version**: 145.0.7578.0 (chromium/7578)

## Additional Resources

- [Official Repository](https://github.com/Dtronix/PDFiumZ)
- [NuGet Package](https://www.nuget.org/packages/PDFiumZ)
- [PDFium Documentation](https://pdfium.googlesource.com/pdfium/)
- [Report Issues](https://github.com/Dtronix/PDFiumZ/issues)
