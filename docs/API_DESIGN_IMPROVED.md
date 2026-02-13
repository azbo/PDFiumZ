# PDFium.Z 高层 API 设计（参考 QuestPDF 风格）

## 设计理念

参考 [QuestPDF](https://www.questpdf.com/) 的简洁 API 设计：
- 链式调用
- 清晰的配置对象
- 简洁的方法重载
- 灵活的输出选项

---

## 核心 API 设计

### 1. 文档图像生成（参考 QuestPDF）

```csharp
namespace PDFiumZ.HighLevel;

/// <summary>
/// PDF 文档类
/// </summary>
public partial class PdfDocument : IDisposable
{
    // ============ 图像生成（QuestPDF 风格）============

    /// <summary>
    /// 将文档的所有页面生成为图像
    /// </summary>
    /// <returns>每页图像的字节数组枚举</returns>
    public IEnumerable<byte[]> GenerateImages();

    /// <summary>
    /// 使用指定设置生成图像
    /// </summary>
    /// <param name="settings">图像生成设置</param>
    /// <returns>每页图像的字节数组枚举</returns>
    public IEnumerable<byte[]> GenerateImages(ImageGenerationSettings settings);

    /// <summary>
    /// 生成图像并保存到文件
    /// </summary>
    /// <param name="fileNameCallback">
    /// 用于生成文件名的回调函数，参数为页面索引
    /// 例如: index => $"page{index}.png"
    /// </param>
    public void GenerateImages(Func<int, string> fileNameCallback);

    /// <summary>
    /// 使用指定设置生成图像并保存到文件
    /// </summary>
    /// <param name="fileNameCallback">文件名回调函数</param>
    /// <param name="settings">图像生成设置</param>
    public void GenerateImages(Func<int, string> fileNameCallback, ImageGenerationSettings settings);

    /// <summary>
    /// 生成图像并保存到指定目录
    /// </summary>
    /// <param name="directory">目标目录</param>
    /// <param name="baseName">基础文件名（默认: "page"）</param>
    /// <param name="settings">可选的图像生成设置</param>
    public void GenerateImagesToDirectory(
        string directory,
        string baseName = "page",
        ImageGenerationSettings? settings = null);
}
```

### 2. 图像生成设置

```csharp
namespace PDFiumZ.HighLevel;

/// <summary>
/// PDF 页面图像生成设置
/// </summary>
public class ImageGenerationSettings
{
    /// <summary>
    /// 图像格式（默认: PNG）
    /// </summary>
    public ImageFormat ImageFormat { get; set; } = ImageFormat.Png;

    /// <summary>
    /// 图像压缩质量（默认: High）
    /// 仅对 JPEG 格式有效
    /// </summary>
    public ImageCompressionQuality ImageCompressionQuality { get; set; } =
        ImageCompressionQuality.High;

    /// <summary>
    /// 渲染 DPI（默认: 288）
    /// 控制输出图像的分辨率。更高的 DPI 产生更高质量的图像，
    /// 但会增加文件大小和处理时间
    /// </summary>
    public float RasterDpi { get; set; } = 288;

    /// <summary>
    /// 背景颜色（默认: null 表示透明）
    /// </summary>
    public Color? BackgroundColor { get; set; } = null;

    /// <summary>
    /// 渲染标志（默认: 渲染注释 + LCD 文本优化）
    /// </summary>
    public PdfRenderFlags RenderFlags { get; set; } =
        PdfRenderFlags.RenderAnnotations | PdfRenderFlags.LcdTextOptimization;

    /// <summary>
    /// 旋转角度（默认: 无旋转）
    /// </summary>
    public PdfRotation Rotation { get; set; } = PdfRotation.Rotate0;
}

/// <summary>
/// 支持的图像格式
/// </summary>
public enum ImageFormat
{
    Png,
    Jpeg,
    Bmp,
    Gif
}

/// <summary>
/// 图像压缩质量等级
/// </summary>
public enum ImageCompressionQuality
{
    Low = 50,
    Medium = 70,
    High = 90,
    Maximum = 100
}
```

### 3. 使用示例

#### 示例 1: 基础用法 - 获取字节数组

```csharp
using PDFiumZ.HighLevel;

// 加载文档
using var document = new PdfDocument("report.pdf");

// 生成所有页面的图像（使用默认设置）
IEnumerable<byte[]> images = document.GenerateImages();

// 保存到数据库或发送到网络
foreach (var imageBytes in images)
{
    // await blobStorage.UploadAsync(imageBytes);
}
```

#### 示例 2: 自定义设置

```csharp
using PDFiumZ.HighLevel;

var settings = new ImageGenerationSettings
{
    ImageFormat = ImageFormat.Png,
    ImageCompressionQuality = ImageCompressionQuality.High,
    RasterDpi = 300  // 高质量输出
};

IEnumerable<byte[]> images = document.GenerateImages(settings);
```

#### 示例 3: 保存到文件（自动命名）

```csharp
using PDFiumZ.HighLevel;

// 简单命名：page0.png, page1.png, page2.png...
document.GenerateImages(imageIndex => $"page{imageIndex}.png");

// 自定义命名模式
document.GenerateImages(imageIndex => $"invoice_page_{imageIndex:D3}.png");
```

#### 示例 4: 保存到目录

```csharp
using PDFiumZ.HighLevel;

// 保存到指定目录
document.GenerateImagesToDirectory("./output", "document");

// 生成文件: ./output/document0.png, ./output/document1.png, ...

// 使用自定义设置
var settings = new ImageGenerationSettings { RasterDpi = 600 };
document.GenerateImagesToDirectory("./high-res", "page", settings);
```

#### 示例 5: 保存到文件 + 自定义设置

```csharp
using PDFiumZ.HighLevel;
using QuestPDF.Infrastructure;  // 如果使用 QuestPDF 的枚举

var settings = new ImageGenerationSettings
{
    ImageFormat = ImageFormat.Png,
    ImageCompressionQuality = ImageCompressionQuality.High,
    RasterDpi = 288
};

// 方式 1: 字节数组
IEnumerable<byte[]> images = document.GenerateImages(settings);

// 方式 2: 直接保存
document.GenerateImages(imageIndex => $"image{imageIndex}.png", settings);
```

---

## 实现示例

### PdfDocument.GenerateImages 实现

```csharp
public partial class PdfDocument : IDisposable
{
    public IEnumerable<byte[]> GenerateImages()
    {
        return GenerateImages(new ImageGenerationSettings());
    }

    public IEnumerable<byte[]> GenerateImages(ImageGenerationSettings settings)
    {
        for (int i = 0; i < PageCount; i++)
        {
            using var page = Pages[i];
            using var bitmap = page.Render(settings);
            using var stream = new MemoryStream();

            switch (settings.ImageFormat)
            {
                case ImageFormat.Png:
                    bitmap.SaveAsPng(stream);
                    break;
                case ImageFormat.Jpeg:
                    bitmap.SaveAsJpeg(stream, (int)settings.ImageCompressionQuality);
                    break;
                case ImageFormat.Bmp:
                    bitmap.SaveAsBmp(stream);
                    break;
            }

            yield return stream.ToArray();
        }
    }

    public void GenerateImages(Func<int, string> fileNameCallback)
    {
        GenerateImages(fileNameCallback, new ImageGenerationSettings());
    }

    public void GenerateImages(Func<int, string> fileNameCallback, ImageGenerationSettings settings)
    {
        for (int i = 0; i < PageCount; i++)
        {
            using var page = Pages[i];
            using var bitmap = page.Render(settings);

            string fileName = fileNameCallback(i);
            string directory = Path.GetDirectoryName(fileName) ?? ".";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            switch (settings.ImageFormat)
            {
                case ImageFormat.Png:
                    bitmap.SaveAsPng(fileName);
                    break;
                case ImageFormat.Jpeg:
                    bitmap.SaveAsJpeg(fileName, (int)settings.ImageCompressionQuality);
                    break;
                case ImageFormat.Bmp:
                    bitmap.SaveAsBmp(fileName);
                    break;
            }
        }
    }

    public void GenerateImagesToDirectory(
        string directory,
        string baseName = "page",
        ImageGenerationSettings? settings = null)
    {
        settings ??= new ImageGenerationSettings();

        GenerateImages(
            index => Path.Combine(directory, $"{baseName}{index}.{GetFileExtension(settings.ImageFormat)}"),
            settings
        );
    }

    private static string GetFileExtension(ImageFormat format) => format switch
    {
        ImageFormat.Png => "png",
        ImageFormat.Jpeg => "jpg",
        ImageFormat.Bmp => "bmp",
        ImageFormat.Gif => "gif",
        _ => "png"
    };
}
```

### PdfPage.Render 更新

```csharp
public partial class PdfPage : IDisposable
{
    internal PdfBitmap Render(ImageGenerationSettings settings)
    {
        // 根据设置的 DPI 计算缩放比例
        float scale = settings.RasterDpi / 72.0f;  // PDF 标准 DPI 是 72

        var width = (int)(Width * scale);
        var height = (int)(Height * scale);

        var bitmap = fpdfview.FPDFBitmapCreateEx(
            width, height,
            (int)FPDFBitmapFormat.BGRA,
            IntPtr.Zero, 0
        );

        if (bitmap == null || bitmap.IsInvalid)
            throw new InvalidOperationException("Failed to create bitmap");

        // 填充背景色
        if (settings.BackgroundColor.HasValue)
        {
            var color = (uint)settings.BackgroundColor.Value.ToArgb();
            fpdfview.FPDFBitmapFillRect(bitmap, 0, 0, width, height, color);
        }

        // 设置变换矩阵
        using var matrix = new FS_MATRIX_();
        using var clipping = new FS_RECTF_();

        matrix.A = scale;
        matrix.B = 0;
        matrix.C = 0;
        matrix.D = scale;
        matrix.E = 0;
        matrix.F = 0;

        clipping.Left = 0;
        clipping.Right = width;
        clipping.Bottom = 0;
        clipping.Top = height;

        // 应用旋转
        int rotateFlag = (int)settings.Rotation;

        // 渲染页面
        fpdfview.FPDF_RenderPageBitmapWithMatrix(
            bitmap, _handle, matrix, clipping,
            rotateFlag | (int)settings.RenderFlags
        );

        return new PdfBitmap(bitmap, width, height);
    }
}
```

---

## 扩展 API 设计

### 4. 单页图像生成

```csharp
public partial class PdfPage : IDisposable
{
    /// <summary>生成图像字节流</summary>
    public byte[] GenerateImage();

    /// <summary>使用设置生成图像字节流</summary>
    public byte[] GenerateImage(ImageGenerationSettings settings);

    /// <summary>保存为图像文件</summary>
    public void SaveAsImage(string filePath);

    /// <summary>使用设置保存为图像文件</summary>
    public void SaveAsImage(string filePath, ImageGenerationSettings settings);
}
```

### 5. 批量处理扩展

```csharp
public static class PdfDocumentExtensions
{
    /// <summary>生成图像并返回字典（页索引 -> 图像字节）</summary>
    public static Dictionary<int, byte[]> GenerateImageDictionary(
        this PdfDocument document,
        ImageGenerationSettings? settings = null)
    {
        var result = new Dictionary<int, byte[]>();
        foreach (var (index, bytes) in document.GenerateImages(settings).Select((bytes, index) => (index, bytes)))
        {
            result[index] = bytes;
        }
        return result;
    }

    /// <summary>异步生成图像</summary>
    public static async Task<List<byte[]>> GenerateImagesAsync(
        this PdfDocument document,
        ImageGenerationSettings? settings = null)
    {
        return await Task.Run(() => document.GenerateImages(settings).ToList());
    }
}
```

---

## API 对比表

| 操作 | QuestPDF | PDFium.Z (建议) |
|------|-----------|-----------------|
| 获取字节数组 | `document.GenerateImages()` | `document.GenerateImages()` |
| 自定义设置 | `document.GenerateImages(settings)` | `document.GenerateImages(settings)` |
| 保存文件 | `document.GenerateImages(nameCallback)` | `document.GenerateImages(nameCallback)` |
| 保存到目录 | N/A | `document.GenerateImagesToDirectory(dir, baseName)` |
| 单页图像 | N/A | `page.GenerateImage()` / `page.SaveAsImage(path)` |

---

## 命名空间组织

```
PDFiumZ                      // 底层 P/Invoke（已存在）
PDFiumZ.HighLevel             // 高层 API（新增）
│   ├── PdfDocument.cs        // 文档类
│   ├── PdfPage.cs            // 页面类
│   ├── PdfBitmap.cs         // 位图类
│   ├── Types/
│   │   ├── ImageGenerationSettings.cs
│   │   ├── ImageFormat.cs
│   │   └── ...
│   └── Extensions/
│       └── PdfDocumentExtensions.cs
```

---

## 迁移建议

### 保持向后兼容

```csharp
// 旧的底层 API 仍然可用
using PDFiumZ;
fpdfview.FPDF_InitLibrary();
// ...

// 新的高层 API
using PDFiumZ.HighLevel;
using var doc = new PdfDocument("file.pdf");
doc.GenerateImages(i => $"page{i}.png");
```

### 渐进式采用

- 新项目直接使用高层 API
- 现有项目可以逐步迁移
- 性能关键场景继续使用底层 API

---

## 来源说明

本文档基于以下资源分析设计：
- [QuestPDF 官方文档](https://www.questpdf.com/concepts/generating-output.html)
- [QuestPDF GitHub 仓库](https://github.com/QuestPDF/QuestPDF)
- [QuestPDF - Generating Output](https://www.questpdf.com/)
- 现有 PDFium.Z 项目架构
