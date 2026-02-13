# PDFium.Z 高层 API 设计建议

## 设计目标

1. **类型安全**：避免使用 `IntPtr` 和 unsafe 指针
2. **资源管理**：使用 `IDisposable` 自动管理 PDFium 资源
3. **易用性**：提供符合 .NET 惯例的 API
4. **可扩展**：保留底层 P/Invoke 访问能力

---

## 核心类设计

### 1. PdfDocument 类

```csharp
namespace PDFiumZ;

/// <summary>
/// 表示 PDF 文档，提供文档级别的操作
/// </summary>
public class PdfDocument : IDisposable
{
    /// <summary>
    /// 从文件路径加载 PDF 文档
    /// </summary>
    /// <param name="filePath">PDF 文件路径</param>
    /// <param name="password">文档密码（如有加密）</param>
    public PdfDocument(string filePath, string? password = null);

    /// <summary>
    /// 从字节流加载 PDF 文档
    /// </summary>
    /// <param name="stream">包含 PDF 数据的流</param>
    /// <param name="password">文档密码（如有加密）</param>
    public PdfDocument(Stream stream, string? password = null);

    /// <summary>
    /// 从字节数组加载 PDF 文档
    /// </summary>
    public PdfDocument(byte[] data, string? password = null);

    // ============ 基本属性 ============

    /// <summary>获取文档页数</summary>
    public int PageCount { get; }

    /// <summary>获取文档是否加密</summary>
    public bool IsEncrypted { get; }

    /// <summary>获取文档权限</summary>
    public PdfPermissions Permissions { get; }

    /// <summary>获取 PDF 版本</summary>
    public PdfVersion Version { get; }

    // ============ 页面访问 ============

    /// <summary>获取文档所有页面的集合</summary>
    public PdfPageCollection Pages { get; }

    /// <summary>按索引获取页面</summary>
    public PdfPage this[int index] => Pages[index];

    // ============ 文档操作 ============

    /// <summary>保存文档到指定路径</summary>
    public void Save(string filePath);

    /// <summary>保存文档到流</summary>
    public void Save(Stream stream);

    /// <summary>删除指定页面</summary>
    public void RemovePage(int index);

    /// <summary>旋转页面</summary>
    public void RotatePage(int index, PdfRotation rotation);

    // ============ 资源释放 ============

    public void Dispose();
}
```

### 2. PdfPage 类

```csharp
namespace PDFiumZ;

/// <summary>
/// 表示 PDF 中的一个页面
/// </summary>
public class PdfPage : IDisposable
{
    internal PdfPage(PdfDocument document, int index);

    // ============ 基本属性 ============

    /// <summary>获取所属文档</summary>
    public PdfDocument Document { get; }

    /// <summary>获取页面索引</summary>
    public int Index { get; }

    /// <summary>获取页面宽度（点）</summary>
    public float Width { get; }

    /// <summary>获取页面高度（点）</summary>
    public float Height { get; }

    /// <summary>获取页面旋转角度</summary>
    public PdfRotation Rotation { get; }

    /// <summary>获取页面边界框</summary>
    public PdfRectangle Bounds { get; }

    // ============ 渲染功能 ============

    /// <summary>
    /// 渲染页面为位图
    /// </summary>
    /// <param name="options">渲染选项</param>
    /// <returns>渲染后的位图</returns>
    public PdfBitmap Render(PdfRenderOptions? options = null);

    /// <summary>
    /// 渲染页面的指定区域
    /// </summary>
    public PdfBitmap Render(PdfRectangle region, PdfRenderOptions? options = null);

    /// <summary>
    /// 渲染页面到指定缩放比例
    /// </summary>
    public PdfBitmap Render(float scale, PdfRenderOptions? options = null);

    // ============ 文本提取 ============

    /// <summary>
    /// 提取页面中的所有文本
    /// </summary>
    public string ExtractText();

    /// <summary>
    /// 提取带位置信息的文本
    /// </summary>
    public IList<PdfTextSegment> ExtractTextWithLocations();

    /// <summary>
    /// 在页面中搜索文本
    /// </summary>
    public IList<PdfTextMatch> Search(string query, PdfSearchOptions? options = null);

    // ============ 图像提取 ============

    /// <summary>
    /// 获取页面中的所有图像
    /// </summary>
    public IList<PdfImage> GetImages();

    // ============ 资源释放 ============

    public void Dispose();
}
```

### 3. PdfRenderOptions 类

```csharp
namespace PDFiumZ;

/// <summary>
/// PDF 页面渲染选项
/// </summary>
public class PdfRenderOptions
{
    /// <summary>渲染缩放比例（默认: 1.0）</summary>
    public float Scale { get; set; } = 1.0f;

    /// <summary>背景颜色（默认: 透明）</summary>
    public Color? BackgroundColor { get; set; }

    /// <summary>旋转角度</summary>
    public PdfRotation Rotation { get; set; } = PdfRotation.Rotate0;

    /// <summary>渲染标志</summary>
    public PdfRenderFlags Flags { get; set; } =
        PdfRenderFlags.RenderAnnotations |
        PdfRenderFlags.LcdTextOptimization;

    /// <summary>DPI（用于计算缩放）</summary>
    public float Dpi { get; set; } = 72.0f;

    /// <summary>根据 DPI 计算缩放比例</summary>
    public float GetScaleForDpi(float targetDpi) => targetDpi / Dpi * Scale;
}
```

### 4. PdfBitmap 类

```csharp
namespace PDFiumZ;

/// <summary>
/// 表示渲染的 PDF 位图
/// </summary>
public class PdfBitmap : IDisposable
{
    internal PdfBitmap(IntPtr bitmapHandle, int width, int height);

    /// <summary>位图宽度（像素）</summary>
    public int Width { get; }

    /// <summary>位图高度（像素）</summary>
    public int Height { get; }

    /// <summary>像素格式</summary>
    public PdfPixelFormat Format { get; }

    /// <summary>获取原始像素数据</summary>
    public ReadOnlySpan<byte> GetData();

    /// <summary>复制像素数据到指定缓冲区</summary>
    public void CopyData(Span<byte> buffer);

    /// <summary>保存为 PNG 文件</summary>
    public void SaveAsPng(string filePath);

    /// <summary>保存为 JPEG 文件</summary>
    public void SaveAsJpeg(string filePath, int quality = 90);

    /// <summary>转换为 System.Drawing.Bitmap（Windows）</summary>
    public System.Drawing.Bitmap ToSystemBitmap();

    /// <summary>转换为 SkiaSharp.SKBitmap（跨平台）</summary>
    public SkiaSharp.SKBitmap ToSkiaBitmap();

    public void Dispose();
}
```

---

## 辅助类型定义

### 枚举类型

```csharp
/// <summary>PDF 旋转角度</summary>
public enum PdfRotation
{
    Rotate0 = 0,      // 0 度
    Rotate90 = 1,     // 90 度
    Rotate180 = 2,    // 180 度
    Rotate270 = 3     // 270 度
}

/// <summary>像素格式</summary>
public enum PdfPixelFormat
{
    Gray,      // 灰度
    Bgr,       // BGR 24位
    Bgra,      // BGRA 32位
    Rgb        // RGB 24位
}

/// <summary>PDF 版本</summary>
public enum PdfVersion
{
    Pdf1_0 = 10,
    Pdf1_1 = 11,
    Pdf1_2 = 12,
    Pdf1_3 = 13,
    Pdf1_4 = 14,
    Pdf1_5 = 15,
    Pdf1_6 = 16,
    Pdf1_7 = 17,
    Pdf2_0 = 20
}

/// <summary>文档权限标志</summary>
[Flags]
public enum PdfPermissions
{
    None = 0,
    Print = 0x0004,
    Modify = 0x0008,
    CopyExtract = 0x0010,
    ModifyAnnotations = 0x0020,
    FillForms = 0x0100,
    ExtractAccessibility = 0x0200,
    Assemble = 0x0400,
    PrintHighQuality = 0x0800
}
```

### 结构体类型

```csharp
/// <summary>PDF 矩形区域</summary>
public readonly struct PdfRectangle
{
    public PdfRectangle(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public float Left { get; }
    public float Top { get; }
    public float Right { get; }
    public float Bottom { get; }

    public float Width => Right - Left;
    public float Height => Bottom - Top;
}

/// <summary>文本片段（带位置信息）</summary>
public class PdfTextSegment
{
    public string Text { get; init; } = string.Empty;
    public PdfRectangle Bounds { get; init; }
    public float FontSize { get; init; }
}

/// <summary>文本匹配结果</summary>
public class PdfTextMatch
{
    public string Text { get; init; } = string.Empty;
    public PdfRectangle Bounds { get; init; }
    public int Index { get; init; }
}
```

---

## 使用示例

### 基本用法

```csharp
// 加载文档
using var pdf = new PdfDocument("document.pdf");

// 遍历所有页面
foreach (var page in pdf.Pages)
{
    Console.WriteLine($"Page {page.Index}: {page.Width} x {page.Height}");
}

// 渲染第一页
using var firstPage = pdf.Pages[0];
using var bitmap = firstPage.Render(new PdfRenderOptions
{
    Scale = 2.0f,
    BackgroundColor = Color.White
});

// 保存为图像
bitmap.SaveAsPng("page1.png");
```

### 文本提取

```csharp
using var pdf = new PdfDocument("document.pdf");
var page = pdf.Pages[0];

// 纯文本提取
string text = page.ExtractText();

// 带位置的文本提取
var segments = page.ExtractTextWithLocations();
foreach (var segment in segments)
{
    Console.WriteLine($"{segment.Text} at {segment.Bounds}");
}
```

### 图像提取

```csharp
using var pdf = new PdfDocument("document.pdf");
var page = pdf.Pages[0];

var images = page.GetImages();
for (int i = 0; i < images.Count; i++)
{
    images[i].SaveAsPng($"image_{i}.png");
}
```

---

## 实现架构建议

### 分层设计

```
┌─────────────────────────────────────┐
│      高层 API (Recommended)        │
│  PdfDocument, PdfPage, PdfBitmap   │
└─────────────────────────────────────┘
           ↓
┌─────────────────────────────────────┐
│     中层封装 (Wrapper Layer)       │
│  Resource management, type safety   │
└─────────────────────────────────────┘
           ↓
┌─────────────────────────────────────┐
│     底层绑定 (Generated P/Invoke)  │
│  PDFiumZ.cs (16412 lines)          │
└─────────────────────────────────────┘
```

### 命名空间组织

```
PDFiumZ                    // 底层 P/Invoke（已存在）
PDFiumZ.HighLevel          // 高层 API（新增）
PDFiumZ.HighLevel.Text     // 文本提取相关
PDFiumZ.HighLevel.Image    // 图像处理相关
PDFiumZ.HighLevel.Form     // 表单处理相关（未来）
PDFiumZ.HighLevel.Annotations // 注释相关（未来）
```

---

## 迁移策略

### 向后兼容

高层 API 应作为可选层，底层 P/Invoke 绑定保持不变：

```csharp
// 用户仍可直接使用底层 API
using PDFium;
fpdfview.FPDF_InitLibrary();
var doc = fpdfview.FPDF_LoadDocument("file.pdf", null);
// ...

// 或使用高层 API
using PDFiumZ.HighLevel;
using var pdf = new PdfDocument("file.pdf");
```

### 命名约定

- 高层 API 使用 .NET 命名约定（PascalCase）
- 底层绑定保留原始 PDFium 命名（保持与 C API 一致）

---

## 来源说明

本文档基于以下资源分析设计：
- [PDFium Google Source](https://pdfium.googlesource.com/pdfium/)
- [PDFium API 文档](https://pdfium.googlesource.com/pdfium/+/refs/heads/main/public/)
- 现有库设计参考：[PdfiumViewer](https://github.com/pvginkel/PdfiumViewer)、[PDFiumSharp](https://github.com/ArgusMagnus/PDFiumSharp)
