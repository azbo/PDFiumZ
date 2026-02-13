# PDFium.Z 高层 API 实现完成

## 概述

PDFium.Z 项目的高层 API 已经成功实现并通过测试。该 API 提供了类似 QuestPDF 的简洁接口来处理 PDF 文档和生成图像。

## 实现的功能

### 1. 核心类

#### PdfDocument
- 从文件路径、字节流或字节数组加载 PDF 文档
- 自动初始化 PDFium 库
- 实现 IDisposable 进行资源管理
- 提供 QuestPDF 风格的图像生成方法

#### PdfPage
- 表示 PDF 中的单个页面
- 获取页面尺寸（Width、Height）
- 渲染页面为位图
- 生成图像字节数组
- 保存图像到文件

#### PdfBitmap
- 包装 PDFium 原生位图
- 支持转换为 System.Drawing.Bitmap
- 提供多种图像格式保存（PNG、JPEG、BMP）
- 获取原始像素数据

#### PdfPageCollection
- 实现 IList<PdfPage> 接口
- 提供页面枚举功能
- 按索引访问页面

### 2. 图像生成 API（QuestPDF 风格）

```csharp
// 返回字节数组枚举
IEnumerable<byte[]> GenerateImages()
IEnumerable<byte[]> GenerateImages(ImageGenerationSettings settings)

// 保存到文件
void GenerateImages(Func<int, string> fileNameCallback)
void GenerateImages(Func<int, string> fileNameCallback, ImageGenerationSettings settings)

// 保存到目录
void GenerateImagesToDirectory(string directory, string baseName = "page", ImageGenerationSettings? settings = null)
```

### 3. 图像生成设置

```csharp
public class ImageGenerationSettings
{
    public ImageFormat ImageFormat { get; set; } = ImageFormat.Png;
    public ImageCompressionQuality ImageCompressionQuality { get; set; } = ImageCompressionQuality.High;
    public float RasterDpi { get; set; } = 288;  // 默认 4x 缩放
    public Color? BackgroundColor { get; set; } = Color.White;
    public PdfRenderFlags RenderFlags { get; set; } =
        PdfRenderFlags.RenderAnnotations | PdfRenderFlags.LcdTextOptimization;
    public PdfRotation Rotation { get; set; } = PdfRotation.Rotate0;
}
```

## 项目结构

```
src/
├── PDFiumZ/                    # 底层 P/Invoke 绑定
├── PDFiumZ.HighLevel/           # 高层 API
│   ├── Types/                   # 枚举和设置类
│   ├── PdfDocument.cs            # 文档类
│   ├── PdfPage.cs               # 页面类
│   ├── PdfBitmap.cs             # 位图类（System.Drawing）
│   └── PdfPageCollection.cs     # 页面集合
├── PDFiumZDemo/                # 原始 Demo
└── PDFiumZDemo.SkiaSharp/      # SkiaSharp Demo（实际使用 System.Drawing）
```

## Demo 示例

Demo 程序包含 6 个示例：

### 示例 1: 基础用法
```csharp
using var document = new PdfDocument("pdf-sample.pdf");
Console.WriteLine($"文档页数: {document.PageCount}");
foreach (var page in document.Pages)
{
    Console.WriteLine($"  页面 {page.Index}: {page.Width:F2} x {page.Height:F2} 点");
}
```

### 示例 2: 生成单页图像
```csharp
using var document = new PdfDocument("pdf-sample.pdf");
using var page = document[0];
page.SaveAsImage("output-single.png");
```

### 示例 3: 生成所有页面图像（字节数组）
```csharp
using var document = new PdfDocument("pdf-sample.pdf");
IEnumerable<byte[]> images = document.GenerateImages();
foreach (var imageBytes in images)
{
    Console.WriteLine($"图像大小: {imageBytes.Length:N0} 字节");
}
```

### 示例 4: 生成并保存所有页面
```csharp
using var document = new PdfDocument("pdf-sample.pdf");
document.GenerateImages(imageIndex => $"page{imageIndex}.png");
```

### 示例 5: 自定义设置生成图像
```csharp
using var document = new PdfDocument("pdf-sample.pdf");
var settings = new ImageGenerationSettings
{
    ImageFormat = ImageFormat.Png,
    ImageCompressionQuality = ImageCompressionQuality.High,
    RasterDpi = 300
};
IEnumerable<byte[]> images = document.GenerateImages(settings);
```

### 示例 6: 保存到目录
```csharp
using var document = new PdfDocument("pdf-sample.pdf");
document.GenerateImagesToDirectory("output", "pdf-page");
```

## 技术细节

### 依赖项
- **PDFiumZ**: 底层 P/Invoke 绑定
- **System.Drawing.Common**: 图像处理（仅 Windows）

### 特性
- 自动资源管理（IDisposable）
- 类型安全的 API
- 异常处理和错误报告
- 支持 PDF 密码保护
- 灵活的图像生成选项

### 限制
- System.Drawing.Common 仅在 Windows 上受支持
- 需要 `AllowUnsafeBlocks=true` 进行原生互操作
- 默认 DPI 288（4x 缩放）以获得高质量输出

## 测试结果

Demo 程序成功运行，所有 6 个示例都正常工作：

1. ✅ 基础用法 - 成功读取 PDF 信息
2. ✅ 单页图像 - 成功生成 output-single.png (351 KB)
3. ✅ 字节数组 - 成功生成图像字节数据 (351,525 字节)
4. ✅ 批量保存 - 成功保存 page0.png, document_page_000.png
5. ✅ 自定义设置 - 使用 DPI 300 成功生成图像
6. ✅ 目录保存 - 成功保存到 output/ 目录

## 后续改进建议

1. **SkiaSharp 支持**: 创建真正的 SkiaSharp 版本以支持跨平台
2. **文本提取**: 添加 PDF 文本提取功能
3. **表单处理**: 支持填写和读取 PDF 表单
4. **页面操作**: 添加页面插入、删除、重新排序
5. **注释处理**: 读取、添加、修改 PDF 注释
6. **性能优化**: 异步 API 和流式处理
7. **ARM 支持**: 添加 ARM64 平台支持

## 相关文档

- [FEATURE_ROADMAP.md](FEATURE_ROADMAP.md) - 功能路线图
- [API_DESIGN.md](API_DESIGN.md) - API 设计文档
- [API_DESIGN_IMPROVED.md](API_DESIGN_IMPROVED.md) - QuestPDF 风格 API 设计
- [QUICK_START.md](QUICK_START.md) - 快速开始指南
