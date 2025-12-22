# PDFiumZ Image Generation API

## 概述

PDFiumZ 提供了简单易用的图片生成API，可以将PDF页面渲染为图片并保存。

## API方法

### 1. GenerateImages() - 生成图片对象

返回IEnumerable<PdfImage>，需要手动处理每个图片：

```csharp
using var document = PdfDocument.Open("sample.pdf");

// 生成所有页面的图片
foreach (var image in document.GenerateImages())
{
    using (image)
    {
        // 手动处理图片...
        image.SaveAsSkiaPng($"page-{i}.png");
    }
}

// 生成指定范围的图片
foreach (var image in document.GenerateImages(startIndex: 0, count: 5))
{
    using (image)
    {
        // 处理前5页...
    }
}

// 使用自定义渲染选项
var options = RenderOptions.Default.WithDpi(150).WithTransparency();
foreach (var image in document.GenerateImages(options))
{
    using (image)
    {
        // 高DPI渲染...
    }
}
```

### 2. SaveAsImages() - 直接保存到文件 ⭐推荐

最简单的API，自动保存所有页面：

```csharp
using var document = PdfDocument.Open("sample.pdf");

// 方式1: 保存到目录（推荐 - 最简单）
// 自动生成文件名: page-0.png, page-1.png, ...
document.SaveAsImages("output/");

// 方式2: 自定义文件名模式
// 生成文件名: document-page-0.png, document-page-1.png, ...
document.SaveAsImages("output/", "document-page-{0}.png");

// 方式3: 保存指定范围
// 只保存第1-3页
document.SaveAsImages("output/", startIndex: 0, count: 3);

// 方式4: 使用自定义路径生成器
// 完全自定义文件名和路径
document.SaveAsImages(pageIndex => $"output/custom-{pageIndex:D3}.png");

// 方式5: 使用高DPI选项
var options = RenderOptions.Default.WithDpi(300);
document.SaveAsImages("highres/", fileNamePattern: "page-{0}.png", options: options);
```

## 完整使用示例

### 示例1: 最简单的用法

```csharp
PdfiumLibrary.Initialize();

try
{
    using var document = PdfDocument.Open("sample.pdf");

    // 一行代码保存所有页面为图片
    var filePaths = document.SaveAsImages("output/");

    Console.WriteLine($"Saved {filePaths.Length} images:");
    foreach (var path in filePaths)
    {
        Console.WriteLine($"  {path}");
    }
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

### 示例2: 自定义文件名

```csharp
PdfiumLibrary.Initialize();

try
{
    using var document = PdfDocument.Open("invoice.pdf");

    // 使用有意义的文件名
    document.SaveAsImages(
        pageIndex => $"output/invoice-{DateTime.Now:yyyyMMdd}-page{pageIndex + 1}.png");
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

### 示例3: 高质量渲染

```csharp
PdfiumLibrary.Initialize();

try
{
    using var document = PdfDocument.Open("presentation.pdf");

    // 300 DPI高清渲染，带透明背景
    var options = RenderOptions.Default
        .WithDpi(300)
        .WithTransparency();

    document.SaveAsImages(
        "highres/",
        fileNamePattern: "slide-{0}.png",
        options: options);
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

### 示例4: 只保存部分页面

```csharp
PdfiumLibrary.Initialize();

try
{
    using var document = PdfDocument.Open("book.pdf");

    // 只保存目录页（假设是前10页）
    document.SaveAsImages(
        "toc/",
        startIndex: 0,
        count: 10,
        fileNamePattern: "toc-page-{0}.png");

    // 只保存第一章（假设是第11-30页）
    document.SaveAsImages(
        "chapter1/",
        startIndex: 10,
        count: 20,
        fileNamePattern: "chapter1-page-{0}.png");
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

### 示例5: 批量处理多个PDF

```csharp
PdfiumLibrary.Initialize();

try
{
    var pdfFiles = Directory.GetFiles("input/", "*.pdf");

    foreach (var pdfFile in pdfFiles)
    {
        var fileName = Path.GetFileNameWithoutExtension(pdfFile);

        using var document = PdfDocument.Open(pdfFile);

        // 为每个PDF创建单独的文件夹
        var outputDir = $"output/{fileName}/";
        document.SaveAsImages(outputDir);

        Console.WriteLine($"Processed: {pdfFile} -> {outputDir}");
    }
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

## API对比

### 旧API（复杂）

```csharp
// 需要手动循环、渲染、保存
for (int i = 0; i < document.PageCount; i++)
{
    using var page = document.GetPage(i);
    using var image = page.RenderToImage();
    image.SaveAsSkiaPng($"output/page-{i}.png");
}
```

### 新API（简单） ⭐

```csharp
// 一行代码完成
document.SaveAsImages("output/");
```

## 参数说明

### RenderOptions

| 方法 | 说明 | 默认值 |
|------|------|--------|
| `WithDpi(int)` | 设置DPI（分辨率） | 96 DPI |
| `WithTransparency()` | 使用透明背景 | 白色背景 |
| `AddFlags(RenderFlags)` | 添加渲染标志 | - |

### 文件名模式

- `{0}` 会被替换为页面索引（从0开始）
- 例如: `"page-{0}.png"` → `page-0.png`, `page-1.png`, ...
- 例如: `"doc-{0:D3}.png"` → `doc-000.png`, `doc-001.png`, ... (3位补零)

## 注意事项

1. **自动创建目录**: `SaveAsImages` 会自动创建不存在的目录
2. **图片格式**: 当前仅支持PNG格式（需要SkiaSharp扩展）
3. **内存管理**: `GenerateImages()` 返回的图片需要手动Dispose
4. **性能**: 大量页面时建议使用 `SaveAsImages()` 而不是手动循环

## 错误处理

```csharp
try
{
    using var document = PdfDocument.Open("sample.pdf");
    document.SaveAsImages("output/");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"PDF file not found: {ex.Message}");
}
catch (PdfException ex)
{
    Console.WriteLine($"PDF error: {ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"IO error: {ex.Message}");
}
```

## 推荐使用方式

✅ **推荐**: 使用 `SaveAsImages()` - 简单、自动管理资源

```csharp
document.SaveAsImages("output/");
```

❌ **不推荐**: 手动循环处理 - 代码冗长、容易出错

```csharp
for (int i = 0; i < document.PageCount; i++)
{
    using var page = document.GetPage(i);
    using var image = page.RenderToImage();
    image.SaveAsSkiaPng($"output/page-{i}.png");
}
```
