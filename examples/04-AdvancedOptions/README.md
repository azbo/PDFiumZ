# 04-AdvancedOptions - 高级选项配置

本目录演示如何使用选项类进行精细控制。

## 📚 示例列表

### OptionsConfig
展示所有选项类的高级用法：
- `ImageGenerationOptions` - 控制页面渲染
- `ImageSaveOptions` - 控制图像保存
- `ThumbnailOptions` - 控制缩略图生成

## 🚀 运行示例

```bash
# 编译
dotnet build

# 运行
dotnet run
```

## 💡 选项类详解

### 1. ImageGenerationOptions

控制 PDF 页面的渲染方式。

#### 静态工厂方法
```csharp
// 渲染所有页面
var options1 = ImageGenerationOptions.ForAllPages();

// 渲染指定页面
var options2 = ImageGenerationOptions.ForPages(new[] { 0, 2, 5 });

// 渲染页面范围
var options3 = ImageGenerationOptions.ForRange(0, 10);

// .NET 8+ Range 语法
var options4 = ImageGenerationOptions.ForRange(..10, pageCount);
```

#### 流畅 API（链式调用）
```csharp
var options = new ImageGenerationOptions()
    .WithPages(new[] { 0, 1, 2 })
    .WithDpi(300)
    .WithTransparency();
```

#### 属性初始化
```csharp
var options = new ImageGenerationOptions
{
    StartIndex = 0,
    Count = 10,
    RenderOptions = RenderOptions.Default.WithDpi(150)
};
```

### 2. ImageSaveOptions

控制图像文件的保存方式。

#### 核心方法
```csharp
// 保存所有页面到目录
var options1 = ImageSaveOptions.ForAllPages("output/");

// 保存指定页面
var options2 = ImageSaveOptions.ForPages("output/", new[] { 0, 2, 5 });

// 自定义路径生成器
var options3 = ImageSaveOptions.WithPathGenerator(
    pageIndex => $"custom/page_{pageIndex}.png"
);
```

#### 链式配置
```csharp
var options = ImageSaveOptions.ForAllPages("output/")
    .WithFileNamePattern("doc-{0:D3}.png")
    .WithDpi(150)
    .WithTransparency();
```

### 3. ThumbnailOptions

控制缩略图的生成方式。

#### 基本配置
```csharp
// 默认缩略图（200px，中等质量）
var options1 = ThumbnailOptions.Default;

// 自定义大小和质量
var options2 = new ThumbnailOptions
{
    MaxWidth = 400,
    Quality = 2  // 0=低, 1=中, 2=高
};
```

#### 预设质量
```csharp
// 快速预览（低质量）
var fast = ThumbnailOptions.Default.WithLowQuality();

// 最佳质量
var best = ThumbnailOptions.Default.WithHighQuality();
```

## 🎯 最佳实践

### 1. 使用静态工厂方法
```csharp
// ✅ 推荐
var options = ImageGenerationOptions.ForRange(0, 10);

// ⚠️ 可以，但不够简洁
var options = new ImageGenerationOptions
{
    StartIndex = 0,
    Count = 10
};
```

### 2. 链式调用提高可读性
```csharp
// ✅ 推荐 - 链式调用
var options = ImageSaveOptions.ForAllPages("output/")
    .WithDpi(300)
    .WithFileNamePattern("page-{0:D3}.png");

// ✅ 也可以 - 静态工厂
var options = ImageSaveOptions.ForAllPages("output/", RenderOptions.Default.WithDpi(300));
```

### 3. 选项复用
```csharp
// 定义一次，多处使用
var highQualityOptions = RenderOptions.Default.WithDpi(300);

var opts1 = ImageGenerationOptions.ForAllPages(highQualityOptions);
var opts2 = ImageSaveOptions.ForAllPages("output/", highQualityOptions);
```

## 📖 更多信息

- [ImageGeneration 示例](../02-Rendering/) - 图像生成实战
- [API 参考](../../docs/API_Reference.md) - 完整 API 文档

---

**返回示例目录**：[README](../README.md)
