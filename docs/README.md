# PDFiumZ 文档与示例 📚

PDFiumZ 是一个现代化的 .NET PDF 处理库，提供完整的高阶 API 和丰富的示例代码。

## 🚀 快速开始

### 安装
```bash
dotnet add package PDFiumZ
```

### 基础使用
```csharp
using PDFiumZ;
using PDFiumZ.HighLevel;

// 初始化库
PdfiumLibrary.Initialize();

try
{
    // 打开并渲染 PDF
    using var document = PdfDocument.Open("sample.pdf");
    using var page = document.GetPage(0);
    using var image = page.RenderToImage();

    // 保存为 PNG
    image.SaveAsSkiaPng("output.png");

    // 提取文本
    var text = page.ExtractText();
}
finally
{
    PdfiumLibrary.Shutdown();
}
```

## 📑 目录结构

### 完整示例代码
每个示例都是独立可运行的项目，包含详细的中文注释：

- **[01-Basics](../examples/01-Basics/)** - 基础入门
  - `GettingStarted.cs` - 快速入门和基础操作演示

- **[02-Rendering](../examples/02-Rendering/)** - 渲染功能
  - `ImageGeneration.cs` - 将 PDF 页面渲染为图像
  - `Thumbnails.cs` - 生成页面缩略图（支持多种规格和质量）

- **[03-PageManipulation](../examples/03-PageManipulation/)** - 页面操作
  - `MergeSplit.cs` - 合并和拆分 PDF 文档
  - `RangeOperations.cs` - 使用 .NET 8+ Range 语法操作页面

- **[04-AdvancedOptions](../examples/04-AdvancedOptions/)** - 高级选项
  - `OptionsConfig.cs` - 使用选项类进行精细控制

### 功能文档
按主题分类的详细文档：

#### 核心功能
- [异步操作](#异步操作) - 现代 async/await API
- [创建 PDF](#创建-pdf) - 从零开始生成文档
- [合并与拆分](#合并与拆分) - 文档组合操作
- [页面旋转](#页面旋转) - 页面方向调整

#### 内容处理
- [渲染与文本提取](#渲染与文本提取) - 页面渲染和内容提取
- [图像生成](#图像生成) - 批量导出页面为图像
- [缩略图生成](#缩略图生成) - 快速预览缩略图
- [图像提取](#图像提取) - 提取嵌入的图片

#### 高级功能
- [表单处理](#表单处理) - 读取和填写表单字段
- [注释功能](#注释功能) - 10+ 种注释类型
- [水印与页眉页脚](#水印与页眉页脚) - 文档标记和装饰
- [HTML 转 PDF](#html-转-pdf) - HTML 内容转换
- [安全信息](#安全信息) - 加密和权限读取

#### 文档生成
- [内容编辑器](#内容编辑器) - 低级内容控制
- [Fluent API](#fluent-api) - 声明式文档生成

#### 集成
- [SkiaSharp 集成](#skiasharp-集成) - 图像格式支持
- [Range 语法](#range-语法) - .NET 8+ 现代语法支持

## 🎯 按需求导航

### 我想要...

#### 渲染 PDF 为图像
👉 查看 **[图像生成文档](#图像生成)**
👉 运行 **[02-Rendering/ImageGeneration.cs](../examples/02-Rendering/ImageGeneration.cs)**

#### 生成页面缩略图
👉 查看 **[缩略图生成文档](#缩略图生成)**
👉 运行 **[02-Rendering/Thumbnails.cs](../examples/02-Rendering/Thumbnails.cs)**

#### 合并或拆分 PDF
👉 查看 **[合并与拆分文档](#合并与拆分)**
👉 运行 **[03-PageManipulation/MergeSplit.cs](../examples/03-PageManipulation/MergeSplit.cs)**

#### 使用 Range 语法 (.NET 8+)
👉 查看 **[Range 语法文档](#range-语法)**
👉 运行 **[03-PageManipulation/RangeOperations.cs](../examples/03-PageManipulation/RangeOperations.cs)**

#### 使用高级配置选项
👉 查看 **[选项类参考](#选项类参考)**
👉 运行 **[04-AdvancedOptions/OptionsConfig.cs](../examples/04-AdvancedOptions/OptionsConfig.cs)**

#### 从头创建 PDF
👉 查看 **[Fluent API](#fluent-api)**

#### 处理表单和注释
👉 查看 **[表单处理](#表单处理)** 和 **[注释功能](#注释功能)**

## 📚 功能详解

---

### 异步操作

PDFiumZ 为大多数长时间运行的操作提供异步版本。

```csharp
using PDFiumZ.HighLevel;

// 异步打开文档
using var document = await PdfDocument.OpenAsync("sample.pdf");
using var page = document.GetPage(0);

// 异步提取和搜索文本
var text = await page.ExtractTextAsync();
var results = await page.SearchTextAsync("PDFiumZ");

// 异步渲染为图像
using var image = await page.RenderToImageAsync();
image.SaveAsSkiaPng("output.png");

// 异步添加水印
await document.AddTextWatermarkAsync("DRAFT", WatermarkPosition.Center);
await document.SaveAsync("output.pdf");
```

---

### 创建 PDF

从零开始创建 PDF 文档。

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();
using var page1 = document.CreatePage(PdfPageSize.A4);
using var page2 = document.CreatePage(PdfPageSize.Letter);
using var page3 = document.CreatePage(800, 600);   // 自定义尺寸

Console.WriteLine($"创建了 {document.PageCount} 页的文档");
document.Save("new-document.pdf");
```

**相关示例**: [01-Basics/GettingStarted.cs](../examples/01-Basics/GettingStarted.cs)

---

### 合并与拆分

合并多个 PDF 或提取特定页面。

```csharp
using PDFiumZ.HighLevel;

// 合并多个文件
using var merged = PdfDocument.Merge("doc1.pdf", "doc2.pdf", "doc3.pdf");
Console.WriteLine($"合并后文档有 {merged.PageCount} 页");
merged.Save("merged.pdf");

// 拆分/提取页面
using var source = PdfDocument.Open("large.pdf");
// 提取页面 0, 1, 和 2
using var first3 = source.Split(0, 1, 2);
first3.Save("first-3-pages.pdf");
```

**相关示例**: [03-PageManipulation/MergeSplit.cs](../examples/03-PageManipulation/MergeSplit.cs)

---

### 页面旋转

调整页面方向。

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");

// 旋转特定页面 (0, 2, 4) 90 度
document.RotatePages(PdfRotation.Rotate90, 0, 2, 4);

// 旋转所有页面 180 度
document.RotatePages(PdfRotation.Rotate180);

// 通过属性旋转单个页面
using var page = document.GetPage(0);
page.Rotation = PdfRotation.Rotate270;

document.Save("rotated.pdf");
```

---

### 渲染与文本提取

渲染页面为图像并提取文本内容。

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);

// 使用自定义 DPI 渲染为图像
using var image = page.RenderToImage(RenderOptions.Default.WithDpi(150));
image.SaveAsSkiaPng("page-0.png");

// 提取纯文本
var text = page.ExtractText();

// 提取带位置和格式的文本
var textPage = page.GetTextPage();
var charCount = textPage.CharCount;
```

---

### 图像生成

将 PDF 页面批量导出为图像。

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("sample.pdf");

// 最简单方式：自动命名 (page-0.png, page-1.png, ...)
document.SaveAsImages("output/");

// 自定义文件名模式
document.SaveAsImages("output/", "document-page-{0}.png");

// 高 DPI 渲染 (300 DPI)
var options = RenderOptions.Default.WithDpi(300);
document.SaveAsImages("highres/", options: options);
```

**相关文档**: [IMAGE_GENERATION.md](./IMAGE_GENERATION.md)

---

### 缩略图生成 ✨ **新功能**

生成页面缩略图，支持多种规格和质量。

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("sample.pdf");

// 为单个页面生成缩略图
using var page = document.GetPage(0);
using var thumbnail = page.GenerateThumbnail(maxWidth: 200);
thumbnail.SaveAsSkiaPng("thumb-page-0.png");

// 为所有页面生成缩略图
var thumbnails = document.GenerateAllThumbnails(maxWidth: 150, quality: 1);
int pageNum = 0;
foreach (var thumb in thumbnails)
{
    using (thumb)
    {
        thumb.SaveAsSkiaPng($"thumbnail-{pageNum++}.png");
    }
}

// 为指定页面生成缩略图
var selectedThumbs = document.GenerateThumbnails(
    pageIndices: new[] { 0, 5, 10 },
    maxWidth: 200,
    quality: 2  // 0=低速/低质, 1=中等, 2=高质量
);

// 不同质量级别
using var lowQuality = page.GenerateThumbnail(maxWidth: 150, quality: 0);    // 快速
using var mediumQuality = page.GenerateThumbnail(maxWidth: 150, quality: 1);  // 默认
using var highQuality = page.GenerateThumbnail(maxWidth: 150, quality: 2);    // 最佳
```

**相关示例**: [02-Rendering/Thumbnails.cs](../examples/02-Rendering/Thumbnails.cs)

---

### 图像提取

从 PDF 页面提取嵌入的图像。

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);

// 提取页面中所有嵌入的图像
var images = page.ExtractImages();
foreach (var img in images)
{
    // img.Image 包含 PdfImage 对象
    // img.Bounds 包含页面上的位置信息
}
```

---

### 表单处理

读取和填写 PDF 表单字段。

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("form.pdf");
using var page = document.GetPage(0);

// 获取所有表单字段
var allFields = page.GetFormFields();
foreach (var field in allFields)
{
    Console.WriteLine($"字段: {field.Name}, 类型: {field.Type}, 值: {field.Value}");
    if (field.Type == FormFieldType.TextField)
        field.SetValue("更新后的值");
}
```

---

### 注释功能

支持 10+ 种注释类型。

#### 读取注释

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("annotated.pdf");
using var page = document.GetPage(0);

// 获取注释数量
var count = page.AnnotationCount;

// 获取所有注释
var allAnnots = page.GetAnnotations();

// 按类型过滤
var highlights = page.GetAnnotations<PdfHighlightAnnotation>();
foreach (var h in highlights)
{
    Console.WriteLine($"高亮位置: {h.Bounds}");
    h.Color = PdfColor.Yellow; // 修改颜色

    // 获取高亮区域
    var regions = h.GetQuadPoints();
}
```

#### 创建注释

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");
using var page = document.GetPage(0);

// 文本标记注释
var highlight = PdfHighlightAnnotation.Create(page,
    new PdfRectangle(100, 700, 200, 20),
    color: 0x80FFFF00); // 半透明黄色

var underline = PdfUnderlineAnnotation.Create(page,
    new PdfRectangle(100, 650, 200, 20));

var strikeout = PdfStrikeOutAnnotation.Create(page,
    new PdfRectangle(100, 600, 200, 20));

// 形状注释
var square = PdfSquareAnnotation.Create(page,
    new PdfRectangle(50, 500, 100, 100),
    strokeColor: PdfColor.Red,
    fillColor: PdfColor.TransparentRed);

var circle = PdfCircleAnnotation.Create(page,
    new PdfRectangle(200, 500, 100, 100),
    strokeColor: PdfColor.Blue);

// 文本注释（便签）
var note = PdfTextAnnotation.Create(page,
    new PdfRectangle(400, 700, 20, 20),
    "这是一个便签");

// 自由文本注释
var textBox = PdfFreeTextAnnotation.Create(page,
    new PdfRectangle(50, 300, 200, 50),
    "可编辑文本框");

// 墨迹注释（手绘）
var ink = PdfInkAnnotation.Create(page);
ink.AddStroke(new[] {
    new PointF(100, 200), new PointF(150, 250),
    new PointF(200, 200)
});

// 图章注释
var stamp = PdfStampAnnotation.Create(page,
    new PdfRectangle(400, 100, 150, 50),
    PdfStampType.Approved);

// 不要忘记释放注释
highlight.Dispose();
// ... 释放其他注释

document.Save("annotated.pdf");
```

---

### 水印与页眉页脚

添加水印、页眉和页脚。

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("document.pdf");

// 文本水印
document.AddTextWatermark(
    "机密",
    WatermarkPosition.Center,
    new WatermarkOptions
    {
        Opacity = 0.3,
        Rotation = 45,
        FontSize = 48,
        Color = PdfColor.Red
    });

// 页眉和页脚
document.AddHeaderFooter(
    headerText: "内部报告 — 第 {page} 页，共 {pages} 页",
    footerText: "© 2023 公司名称",
    options: new HeaderFooterOptions { FontSize = 10, Margin = 36 });

document.Save("protected.pdf");
```

---

### HTML 转 PDF

将 HTML/CSS 转换为 PDF 文档。

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();

string html = @"
    <h1 style='color: #0066CC;'>欢迎使用 PDFiumZ</h1>
    <p>轻松将 <b>HTML</b> 转换为 <i>PDF</i>！</p>
    <table border='1'>
        <tr><th>项目</th><th>价格</th></tr>
        <tr><td>组件</td><td>¥10</td></tr>
    </table>";

document.CreatePageFromHtml(html, new HtmlToPdfOptions {
    Margin = new PdfMargins(36),
    PageSize = PdfPageSize.A4
});

document.Save("html-output.pdf");
```

---

### 安全信息

读取 PDF 安全设置，包括加密状态和权限。

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.Open("protected.pdf");
var security = document.Security;

// 检查加密状态
Console.WriteLine($"已加密: {security.IsEncrypted}");
Console.WriteLine($"用户密码: {security.HasUserPassword}");
Console.WriteLine($"所有者密码: {security.HasOwnerPassword}");

// 检查权限（允许的操作）
Console.WriteLine($"可打印: {security.CanPrint}");
Console.WriteLine($"可修改: {security.CanModify}");
Console.WriteLine($"可复制: {security.CanCopy}");
Console.WriteLine($"可注释: {security.CanAnnotate}");
Console.WriteLine($"可填写表单: {security.CanFillForms}");
Console.WriteLine($"可提取内容: {security.CanExtractContent}");
Console.WriteLine($"可组装文档: {security.CanAssembleDocument}");
Console.WriteLine($"可高质量打印: {security.CanPrintHighQuality}");

// 获取原始权限标志
PdfPermissions permissions = security.Permissions;
```

**注意**: PDFium 仅支持**读取**安全信息，不支持设置密码或加密。这是 PDFium 的限制。

---

### 内容编辑器

使用 `PdfContentEditor` 进行精确的页面内容控制。

```csharp
using PDFiumZ.HighLevel;

using var document = PdfDocument.CreateNew();
using var page = document.CreatePage();
using var font = PdfFont.Load(document, PdfStandardFont.Helvetica);

using (var editor = page.BeginEdit())
{
    editor
        .WithFont(font)
        .WithFontSize(24)
        .WithTextColor(PdfColor.DarkBlue)
        .Text("Hello World", 50, 750)

        .WithStrokeColor(PdfColor.Red)
        .Rectangle(50, 700, 100, 50)

        .Commit();
}
```

---

### Fluent API

用于高级声明式文档生成。详见 [Fluent API 指南](./FLUENT_API.md)。

```csharp
using PDFiumZ.Fluent;
using PDFiumZ.Fluent.Document;

using var document = new FluentDocument();
document.Content(page => {
    page.Column(col => {
        col.Item().Text("标题").FontSize(20).SemiBold();
        col.Item().PaddingVertical(10).LineHorizontal(1);
        col.Item().Text("这是一个声明式文档生成示例。");
    });
});
document.Generate();
document.Save("fluent.pdf");
```

---

### Range 语法 (.NET 8+)

使用现代 Range 语法进行页面操作。

```csharp
// 获取前 10 页
using var pages = document.GetPages(..10);

// 获取最后 5 页
using var pages = document.GetPages(^5..);

// 获取页面 5-15
using var pages = document.GetPages(5..15);

// 删除前 3 页
document.DeletePages(..3);

// 移动最后 5 页到开头
document.MovePages(0, ^5..);
```

**相关文档**: [RangeSupportExamples.md](./RangeSupportExamples.md)
**相关示例**: [03-PageManipulation/RangeOperations.cs](../examples/03-PageManipulation/RangeOperations.cs)

---

### SkiaSharp 集成

PDFiumZ 使用 SkiaSharp 进行渲染和图像处理。

```csharp
using PDFiumZ.HighLevel;
using PDFiumZ.SkiaSharp;
using SkiaSharp;

using var document = PdfDocument.Open("sample.pdf");
using var page = document.GetPage(0);
using var image = page.RenderToImage();

// 保存为不同格式
image.SaveAsSkiaJpeg("output.jpg", quality: 90);
image.SaveAsSkiaWebP("output.webp");

// 直接使用 SKBitmap
SKBitmap bitmap = image.ToSKBitmap();
```

---

## 🎯 学习路径

### 初学者
1. **[01-Basics/GettingStarted.cs](../examples/01-Basics/GettingStarted.cs)** - 运行基础示例
2. 阅读本文档的"快速开始"部分
3. 尝试修改示例代码

### 进阶开发者
1. **[02-Rendering/](../examples/02-Rendering/)** - 渲染相关示例
2. **[03-PageManipulation/](../examples/03-PageManipulation/)** - 页面操作示例
3. 阅读 [IMAGE_GENERATION.md](./IMAGE_GENERATION.md)

### 高级用户
1. **[04-AdvancedOptions/](../examples/04-AdvancedOptions/)** - 高级选项示例
2. 阅读 [FLUENT_API.md](./FLUENT_API.md)
3. 探索 [RangeSupportExamples.md](./RangeSupportExamples.md)

---

## 🔗 相关资源

- **[GitHub 仓库](https://github.com/yourusername/PDFiumZ)** - 源代码和问题追踪
- **[完整示例代码](../examples/)** - 所有示例项目
- **[API 快速参考](./Reference/API_Quick_Reference.md)** - 常用 API 查询
- **[更新日志](../CHANGELOG.md)** - 版本更新历史

---

**PDFiumZ** - .NET 的现代化 PDF 处理库