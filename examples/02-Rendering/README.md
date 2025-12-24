# 02-Rendering - PDF 渲染功能

本目录演示如何将 PDF 页面渲染为图像。

## 📚 示例列表

### ImageGeneration
将 PDF 页面转换为图像的完整示例：
- 保存所有页面为 PNG
- 自定义文件名模式
- 使用自定义路径生成器
- 保存指定范围的页面
- 高 DPI 渲染（300 DPI）
- 透明背景渲染
- 手动处理生成的图像

### Thumbnails
生成 PDF 缩略图的多种方式：
- 默认缩略图（200px，中等质量）
- 高质量缩略图（400px，最高质量）
- 快速预览缩略图（150px，低质量）
- 为指定页面生成缩略图
- 批量生成多种规格
- 使用 ThumbnailOptions 高级配置

## 🚀 运行示例

```bash
# 编译
dotnet build

# 运行
dotnet run
```

**注意**：需要 `sample.pdf` 文件在同一目录下。

## 💡 核心概念

### 1. 简单保存（推荐）
```csharp
using var document = PdfDocument.Open("sample.pdf");
var paths = document.SaveAsImages(ImageSaveOptions.ForAllPages("output/"));
```

### 2. 自定义配置
```csharp
var options = ImageSaveOptions.ForAllPages("output/")
    .WithFileNamePattern("page-{0:D3}.png")
    .WithDpi(300)
    .WithTransparency();

document.SaveAsImages(options);
```

### 3. 手动处理
```csharp
foreach (var image in document.GenerateImages(ImageGenerationOptions.ForAllPages()))
{
    using (image)
    {
        // 自定义处理逻辑
        image.SaveAsPng($"custom-{index}.png");
    }
}
```

## 🔧 相关类

- `ImageSaveOptions` - 控制图像保存行为
- `ImageGenerationOptions` - 控制页面渲染
- `RenderOptions` - 控制渲染质量和样式
- `PdfImage` - 表示渲染后的图像

## 📖 更多信息

- [OptionsConfig 示例](../04-AdvancedOptions/) - 了解所有选项配置
- [文档：图像生成](../../docs/Features/Image_Generation.md)

---

**返回示例目录**：[README](../README.md)
