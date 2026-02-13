# PDFium.Z 高层 API 实现总结

## 已完成

### 1. 项目结构
```
PDFiumZ/
├── PDFiumZ.csproj          # 底层 P/Invoke 绑定（已存在）
└── src/
    ├── PDFiumZ.HighLevel/    # 高层 API（新建）
    │   ├── Types/            # 枚举和设置类
    │   ├── PdfDocument.cs    # 文档类
    │   ├── PdfPage.cs        # 页面类
    │   ├── PdfPageCollection.cs
    │   └── PdfBitmap.cs      # 位图类
    └── PDFiumZDemo/           # 示例程序（更新）
```

### 2. QuestPDF 风格 API 设计

#### 核心方法

```csharp
// 方式 1: 获取字节数组
IEnumerable<byte[]> images = document.GenerateImages();

// 方式 2: 使用设置
var settings = new ImageGenerationSettings
{
    ImageFormat = ImageFormat.Png,
    RasterDpi = 300
};
IEnumerable<byte[]> images = document.GenerateImages(settings);

// 方式 3: 保存到文件（自动命名）
document.GenerateImages(index => $"page{index}.png");

// 方式 4: 保存到目录
document.GenerateImagesToDirectory("./output", "page");
```

#### 配置类

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| ImageFormat | enum | Png | 图像格式 |
| ImageCompressionQuality | enum | High | JPEG 质量 |
| RasterDpi | float | 288 | 渲染 DPI |
| BackgroundColor | Color? | White | 背景颜色 |
| RenderFlags | enum | ... | 渲染标志 |
| Rotation | enum | Rotate0 | 旋转角度 |

---

## 下一步：切换到 SkiaSharp

当前使用 `System.Drawing.Common`，它有以下限制：
1. **仅限 Windows** - Linux/macOS 需要单独配置
2. **即将弃用** - .NET 6+ 仅作为 Windows 包提供

### SkiaSharp 优势

- **跨平台** - 支持 Windows/Linux/macOS
- **现代 API** - 符合 .NET 最佳实践
- **性能更好** - 硬件加速支持
- **长期支持** - .NET 官方推荐

### 需要修改的文件

| 文件 | 修改内容 |
|------|----------|
| `PDFiumZ.HighLevel/PdfBitmap.cs` | 替换 System.Drawing 为 SkiaSharp |
| `PDFiumZ.HighLevel/PdfBitmap.cs` | 添加 SkiaSharp.Bitmap 转换 |
| `PDFiumZ.HighLevel/PdfDocument.cs` | 添加 ToSKBitmap() 扩展方法 |

---

## 运行时问题

### 错误原因

```
Unhandled exception. System.InvalidOperationException: Failed to load PDF document
```

### 可能原因

1. **pdf-sample.pdf 不存在** - Demo 项目需要此文件
2. **PDFium 原生库缺失** - 需要 `pdfium.dll` 在运行时可用

### 解决方案

1. 确认 `pdf-sample.pdf` 在项目根目录
2. 检查 PDFium 库是否正确部署
3. 尝试使用绝对路径加载文件

---

## 建议

### 立即可做

1. **解决运行时问题** - 确保 PDF 库存在
2. **切换到 SkiaSharp** - 替换 System.Drawing
3. **添加 NuGet 包引用** - `SkiaSharp`
4. **完善单元测试** - 添加更多测试用例

### 未来功能扩展

参考 `FEATURE_ROADMAP.md` 文档中的优先级：
- **第一优先级**：高层 API 封装、文本提取、图像提取
- **第二优先级**：表单交互、注释书签、页面操作
