# API 快速参考

PDFiumZ 常用 API 快速查询手册。

## 📑 目录

- [PdfDocument - 文档操作](#pdfdocument)
- [PdfPage - 页面操作](#pdfpage)
- [图像生成](#图像生成)
- [页面操作](#页面操作)
- [选项类](#选项类)

---

## PdfDocument

### 创建和打开

```csharp
// 创建新文档
using var doc = PdfDocument.CreateNew();

// 打开现有文档
using var doc = PdfDocument.Open("file.pdf");

// 从内存打开
using var doc = PdfDocument.Open(stream);

// 合并多个文档
using var merged = PdfDocument.Merge("doc1.pdf", "doc2.pdf");
```

### 基本属性

```csharp
int pageCount = document.PageCount;
var metadata = document.Metadata;
var security = document.Security;
```

### 保存

```csharp
// 保存到文件
document.Save("output.pdf");

// 增量保存（更快）
document.Save("output.pdf", incremental: true);
```

---

## PdfPage

### 获取页面

```csharp
// 获取单个页面
using var page = document.GetPage(0);

// 获取多个页面
var pages = document.GetPages(0, 10);

// .NET 8+ Range 语法
var pages = document.GetPages(..10);
```

### 页面属性

```csharp
int pageIndex = page.Index;
double width = page.Width;
double height = page.Height;
PdfRotation rotation = page.Rotation;
```

### 文本提取

```csharp
// 提取纯文本
string text = page.ExtractText();

// 提取带格式的文本
using var textPage = page.GetTextPage();
```

### 渲染

```csharp
// 简单渲染
using var image = page.RenderToImage();

// 自定义渲染
using var image = page.RenderToImage(RenderOptions.Default.WithDpi(300));

// 生成缩略图
using var thumbnail = page.GenerateThumbnail(maxWidth: 200);
```

---

## 图像生成

### SaveAsImages - 保存为图像文件

```csharp
// 最简单方式
var paths = document.SaveAsImages(ImageSaveOptions.ForAllPages("output/"));

// 自定义文件名
document.SaveAsImages(new ImageSaveOptions
{
    OutputDirectory = "output/",
    FileNamePattern = "page-{0:D3}.png"
});

// 高 DPI
document.SaveAsImages(
    ImageSaveOptions.ForAllPages("output/").WithDpi(300)
);

// 保存指定页面
document.SaveAsImages(
    ImageSaveOptions.ForRange("output/", 0, 10)
);
```

### GenerateImages - 生成图像对象

```csharp
// 生成所有页面
foreach (var image in document.GenerateImages(ImageGenerationOptions.ForAllPages()))
{
    using (image)
    {
        image.SaveAsPng($"page-{index}.png");
    }
}

// 生成指定页面
var options = ImageGenerationOptions.ForPages(new[] { 0, 2, 5 });
foreach (var image in document.GenerateImages(options))
{
    // 处理图像
}
```

### GenerateThumbnails - 生成缩略图

```csharp
// 生成所有页面的缩略图
var options = new ThumbnailOptions { MaxWidth = 200, Quality = 1 };
foreach (var thumb in document.GenerateThumbnails(options))
{
    using (thumb)
    {
        thumb.SaveAsPng($"thumb-{index}.png");
    }
}

// 生成指定页面的缩略图
var options = ThumbnailOptions.Default
    .WithMaxWidth(400)
    .WithPages(new[] { 0, 1, 2 });
```

---

## 页面操作

### 旋转

```csharp
// 旋转指定页面
document.RotatePages(PdfRotation.Rotate90, 0, 2, 4);

// 旋转所有页面
document.RotatePages(PdfRotation.Rotate180);

// 旋转单个页面
using var page = document.GetPage(0);
page.Rotation = PdfRotation.Rotate270;
```

### 删除

```csharp
// 删除指定页面
document.DeletePages(0, 1, 2);

// .NET 8+ Range 语法
document.DeletePages(..3);  // 删除前 3 页
```

### 移动

```csharp
// 移动页面到新位置
document.MovePages(destinationIndex: 0, pageIndices: 5, 6, 7);

// .NET 8+ Range 语法
document.MovePages(0, ^5..);  // 将最后 5 页移到开头
```

### 合并和拆分

```csharp
// 合并文档
using var merged = PdfDocument.Merge("doc1.pdf", "doc2.pdf");

// 拆分文档 - 提取指定页面
using var extracted = document.Split(0, 1, 2);

// .NET 8+ Range 语法
using var extracted = document.Split(5..15);
```

---

## 选项类

### ImageGenerationOptions

```csharp
// 静态工厂方法
var opts1 = ImageGenerationOptions.ForAllPages();
var opts2 = ImageGenerationOptions.ForPages(new[] { 0, 2, 5 });
var opts3 = ImageGenerationOptions.ForRange(0, 10);

// 链式配置
var opts4 = new ImageGenerationOptions()
    .WithPages(new[] { 0, 1 })
    .WithDpi(300)
    .WithTransparency();

// .NET 8+ Range 语法
var opts5 = ImageGenerationOptions.ForRange(..10, pageCount);
```

### ImageSaveOptions

```csharp
// 静态工厂方法
var opts1 = ImageSaveOptions.ForAllPages("output/");
var opts2 = ImageSaveOptions.ForPages("output/", new[] { 0, 2 });
var opts3 = ImageSaveOptions.ForRange("output/", 0, 10);
var opts4 = ImageSaveOptions.WithPathGenerator(idx => $"path/{idx}.png");

// 链式配置
var opts5 = ImageSaveOptions.ForAllPages("output/")
    .WithFileNamePattern("page-{0:D3}.png")
    .WithDpi(300)
    .WithTransparency();
```

### ThumbnailOptions

```csharp
// 基本配置
var opts1 = new ThumbnailOptions { MaxWidth = 200, Quality = 1 };

// 链式配置
var opts2 = ThumbnailOptions.Default
    .WithMaxWidth(400)
    .WithHighQuality()
    .WithPages(new[] { 0, 1, 2 });

// 预设质量
var opts3 = ThumbnailOptions.Default.WithLowQuality();
var opts4 = ThumbnailOptions.Default.WithMediumQuality();
var opts5 = ThumbnailOptions.Default.WithHighQuality();
```

### RenderOptions

```csharp
// 基本配置
var opts1 = RenderOptions.Default.WithDpi(300);
var opts2 = RenderOptions.Default.WithTransparency();
var opts3 = RenderOptions.Default.WithScale(2.0);

// 组合配置
var opts4 = RenderOptions.Default
    .WithDpi(300)
    .WithTransparency()
    .WithFlags(RenderFlags.RenderAnnotations);
```

---

## 常用模式

### 模式 1：处理整个文档

```csharp
using var document = PdfDocument.Open("file.pdf");

for (int i = 0; i < document.PageCount; i++)
{
    using var page = document.GetPage(i);
    // 处理页面
}

document.Save("output.pdf");
```

### 模式 2：批量生成图像

```csharp
using var document = PdfDocument.Open("file.pdf");
var options = ImageSaveOptions.ForAllPages("output/")
    .WithDpi(150)
    .WithFileNamePattern("page-{0:D3}.png");

var paths = document.SaveAsImages(options);
Console.WriteLine($"Generated {paths.Length} images");
```

### 模式 3：处理指定页面

```csharp
using var document = PdfDocument.Open("file.pdf");
var options = ImageGenerationOptions.ForPages(new[] { 0, 5, 10 });

foreach (var image in document.GenerateImages(options))
{
    using (image)
    {
        // 处理图像
    }
}
```

---

## 更多信息

- [完整示例](../examples/)
- [功能文档](./Features/)
- [选项类详解](./Options_Classes.md)

---

**返回文档目录**：[README](../README.md)
