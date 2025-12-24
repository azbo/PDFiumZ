# PDFiumZ Range/Index Support Examples

## Overview

PDFiumZ now supports C# 8+ Range and Index syntax for page operations, making your code more concise and expressive.

## Requirements

- **.NET 8.0 or later** for Range syntax support
- Older frameworks (.NET Standard 2.0/2.1) can still use traditional index-based methods

## Basic Range Syntax

### From the Start (Prefix)

```csharp
// Get first 10 pages
using var pages = document.GetPages(..10);

// Generate images for first 5 pages
var options = ImageGenerationOptions.ForRange(..5, document.PageCount);
foreach (var image in document.GenerateImages(options))
{
    image.Dispose();
}

// Delete first 3 pages
document.DeletePages(..3);
```

### To the End (Suffix)

```csharp
// Get last 5 pages
using var pages = document.GetPages(^5..);

// Get all pages except last 10
using var pages = document.GetPages(..^10);

// Move last 3 pages to the beginning
document.MovePages(0, ^3..);
```

### Middle Range

```csharp
// Get pages 5-15 (indices 5 through 14)
using var pages = document.GetPages(5..15);

// Delete pages 10-20
document.DeletePages(10..20);

// Move pages 3-7 to position 20
document.MovePages(20, 3..7);
```

### Open-Ended Ranges

```csharp
// Get from page 5 to the end
using var pages = document.GetPages(5..);

// Get from start to page 10
using var pages = document.GetPages(..10);

// Move all pages from index 5 to the end
document.MovePages(0, 5..);
```

## Real-World Examples

### Example 1: Process First N Pages

```csharp
using PDFiumZ;
using PDFiumZ.HighLevel;

// Open document
using var document = PdfDocument.Open("large.pdf");

// Process only first 10 pages
var options = ImageGenerationOptions.ForRange(..10, document.PageCount);
foreach (var image in document.GenerateImages(options))
{
    image.SaveAsPng($"thumbnail_{image.PageIndex}.png");
    image.Dispose();
}
```

### Example 2: Remove Cover Pages

```csharp
// Delete first 2 pages (cover, table of contents)
document.DeletePages(..2);
document.Save("document_no_cover.pdf");
```

### Example 3: Extract Last N Pages

```csharp
// Create a new document with only last 5 pages
using var extractedDoc = document.Split(^5..);
extractedDoc.Save("last_5_pages.pdf");
```

### Example 4: Reorganize Pages

```csharp
// Move last 5 pages to the beginning
document.MovePages(0, ^5..);

// Move first 3 pages after page 10
document.MovePages(10, ..3);

document.Save("reorganized.pdf");
```

### Example 5: Process Document in Chunks

```csharp
// Process 100 pages at a time
int chunkSize = 100;
for (int start = 0; start < document.PageCount; start += chunkSize)
{
    int end = Math.Min(start + chunkSize, document.PageCount);
    var range = start..end;

    var options = ImageGenerationOptions.ForRange(range, document.PageCount);
    await ProcessChunkAsync(document, options);
}
```

### Example 6: Delete Footer Pages

```csharp
// Remove last 3 pages from every section
// (assuming 10 pages per section)
int sectionSize = 10;
for (int i = 0; i < document.PageCount; i += sectionSize)
{
    int end = Math.Min(i + sectionSize, document.PageCount);
    // Delete last 3 pages of this section
    document.DeletePages((end - 3)..end);
}
```

## Comparison: Old vs New Syntax

### Getting Pages

```csharp
// Old way
var pages1 = document.GetPages(0, 10);           // First 10 pages
var pages2 = document.GetPages(5, 10);           // Pages 5-14
var pages3 = document.GetPages(document.PageCount - 5, 5); // Last 5 pages

// New way (more expressive)
var pages1 = document.GetPages(..10);            // First 10 pages
var pages2 = document.GetPages(5..15);           // Pages 5-14
var pages3 = document.GetPages(^5..);            // Last 5 pages
```

### Deleting Pages

```csharp
// Old way
document.DeletePages(Enumerable.Range(0, 10).ToArray());

// New way
document.DeletePages(..10);
```

### Moving Pages

```csharp
// Old way
var indices = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
document.MovePages(20, indices);

// New way
document.MovePages(20, ..10);
```

### Generating Images

```csharp
// Old way
var options = ImageGenerationOptions.ForRange(5, 10);

// New way (.NET 8+)
var options = ImageGenerationOptions.ForRange(5..15, document.PageCount);
```

## Performance Considerations

Range operations are **zero-cost abstractions** - they compile down to the same efficient code as traditional index-based methods:

```csharp
// These two are equivalent in performance:
var pages1 = document.GetPages(0, 10);
var pages2 = document.GetPages(..10);
```

The Range syntax is converted to offset and length at compile time using `GetOffsetAndLength()`.

## Framework Compatibility

| Feature | .NET Standard 2.0 | .NET Standard 2.1 | .NET 8+ |
|---------|------------------|------------------|---------|
| Traditional methods | ✅ | ✅ | ✅ |
| Range syntax | ❌ | ❌ | ✅ |
| IAsyncEnumerable | ❌ | ✅ | ✅ |

## Migration Guide

### If you're using .NET 8+

Replace index/count calls with Range syntax where it improves readability:

```csharp
// Before
document.GetPages(0, 10)
document.GetPages(5, 10)
document.GetPages(document.PageCount - 5, 5)

// After
document.GetPages(..10)
document.GetPages(5..15)
document.GetPages(^5..)
```

### If you're on .NET Standard 2.0/2.1

Continue using traditional methods - they work great and will continue to be supported:

```csharp
document.GetPages(0, 10);  // Still works perfectly
```

## Tips and Best Practices

1. **Use Range when it improves readability**
   ```csharp
   // Good - clear intent
   document.GetPages(^5..);  // "Last 5 pages"

   // Also fine - explicit range
   document.GetPages(5..10); // "Pages 5-9"
   ```

2. **Combine with other features**
   ```csharp
   // Range + Async
   var options = ImageGenerationOptions.ForRange(..10, document.PageCount);
   await foreach (var image in document.GenerateImagesAsync(options))
   {
       // Process
   }
   ```

3. **Use descriptive variable names**
   ```csharp
   Range coverPages = ..3;
   Range mainContent = 3..^2;
   Range backMatter = ^2..;
   ```

## Summary

Range/Index support provides:
- ✅ More concise syntax
- ✅ Better expressiveness
- ✅ Zero performance overhead
- ✅ Full backward compatibility
- ✅ Compile-time safety

The traditional index-based methods remain fully supported for all frameworks.
