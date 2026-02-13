# PDFium.Z

[![Version](https://img.shields.io/nuget/v/PDFiumZ)](https://www.nuget.org/packages/PDFiumZ)
[![Downloads](https://img.shields.io/nuget/dt/PDFiumZ)](https://www.nuget.org/packages/PDFiumZ)

## 简介

PDFium.Z 是一个 .NET 库，通过 P/Invoke 绑定提供 PDFium 的完整功能。PDFium 是 Chromium 项目的 PDF 渲染引擎，由 Google 维护，具有高性能和可靠性。

本项目提供了两层 API 设计：
- **底层 API（PDFiumZ）**：完整的 PDFium C API 绑定，适合需要精细控制的场景
- **高层 API（PDFiumZ.HighLevel）**：符合 .NET 习惯的高级封装，简化常见操作

## 特性

- **跨平台支持**：Windows（x86/x64）、Linux（x64）、macOS（x64）
- **多框架支持**：.NET 8.0、.NET 9.0、.NET 10.0、.NET Standard 2.0/2.1
- **双层 API 设计**：底层 P/Invoke 绑定 + 高级 .NET 封装
- **高质量 PDF 渲染**：支持 DPI 控制、旋转、背景色等
- **完整的图像生成支持**：PNG、JPEG、BMP 格式
- **资源自动管理**：实现 IDisposable 接口，确保资源正确释放
- **SkiaSharp 集成**：支持与 SkiaSharp 图形库配合使用

## 安装

通过 NuGet 安装：

```bash
# 核心绑定库
dotnet add package PDFiumZ

# 高级 API（推荐）
dotnet add package PDFiumZ.HighLevel
```

或通过 Package Manager Console：

```powershell
Install-Package PDFiumZ
Install-Package PDFiumZ.HighLevel
```

## 快速开始

### 使用高级 API（推荐）

```csharp
using PDFiumZ.HighLevel;

// 从文件加载 PDF
using var document = new PdfDocument("document.pdf");

// 遍历所有页面
foreach (var page in document.Pages)
{
    // 获取页面尺寸
    var size = page.Size;

    // 渲染页面为图像
    using var bitmap = page.Render();

    // 保存为 PNG 文件
    bitmap.Save("page.png");
}
```

### 自定义渲染设置

```csharp
using PDFiumZ.HighLevel;

using var document = new PdfDocument("document.pdf");
using var page = document.Pages[0];

// 自定义渲染设置
var settings = new ImageGenerationSettings
{
    Dpi = 300,                    // 设置 DPI
    Rotation = PdfRotation.Rotate90,  // 旋转 90 度
    BackgroundColor = SKColors.White,
    Flags = PdfRenderFlags.Grayscale  // 灰度渲染
};

using var bitmap = page.Render(settings);
bitmap.Save("page-grayscale.png", ImageFormat.Png);
```

### 从流加载 PDF

```csharp
using PDFiumZ.HighLevel;

using var stream = File.OpenRead("document.pdf");
using var document = new PdfDocument(stream);

Console.WriteLine($"文档共 {document.PageCount} 页");
```

### 使用底层 API

```csharp
using PDFiumZ;

// 初始化库
fpdfview.FPDF_InitLibrary();

// 加载文档
var document = fpdfview.FPDF_LoadDocument("document.pdf", null);
int pageCount = fpdfview.FPDF_GetPageCount(document);

// 加载第一页
var page = fpdfview.FPDF_LoadPage(document, 0);

// 获取页面尺寸
double width = fpdfview.FPDF_GetPageWidth(page);
double height = fpdfview.FPDF_GetPageHeight(page);

// 清理资源
fpdfview.FPDF_ClosePage(page);
fpdfview.FPDF_CloseDocument(document);
fpdfview.FPDF_DestroyLibrary();
```

## API 文档

### 高级 API 类

| 类 | 说明 |
|---|---|
| `PdfDocument` | 表示 PDF 文档，提供文档级别的操作 |
| `PdfPage` | 表示 PDF 页面，提供页面渲染和操作功能 |
| `PdfPageCollection` | 页面集合，支持索引访问 |
| `PdfBitmap` | 位图对象，用于图像生成和保存 |

### 主要功能

- **文档加载**：支持从文件路径、流、字节数组加载
- **密码保护**：支持加密的 PDF 文档
- **页面渲染**：支持自定义 DPI、旋转、背景色
- **图像格式**：PNG、JPEG、BMP
- **资源管理**：自动资源清理，防止内存泄漏

## 示例项目

### PDFiumZDemo

基础用法演示，包含：
- 文档加载和遍历
- 页面渲染和图像保存
- 基本错误处理

### PDFiumZDemo.SkiaSharp

SkiaSharp 集成示例，展示：
- 与 SkiaSharp 图形库的配合使用
- 高级图像处理
- 自定义渲染管道

## 开发状态

当前版本处于 **Beta** 阶段。核心功能已实现，但 API 可能会在后续版本中调整。

### 已实现功能

- PDF 文档加载（文件、流、字节数组）
- 页面遍历和访问
- 页面渲染（支持 DPI、旋转、背景色）
- 图像生成（PNG、JPEG、BMP）
- 资源自动管理
- 密码保护的 PDF 支持

### 计划中的功能

- 文档元数据提取（标题、作者、创建日期等）
- 文本提取
- 表单操作（读取、填写、扁平化）
- PDF 合并与拆分
- 页面注释和链接
- 更完善的错误处理

## 贡献指南

欢迎贡献！请遵循以下步骤：

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

### 开发环境要求

- .NET SDK 9.0 或更高版本
- 支持的操作系统：Windows、Linux、macOS

### 构建项目

```bash
# 克隆仓库
git clone https://github.com/Dtronix/PDFiumZ.git
cd PDFiumZ/src

# 还原依赖
dotnet restore

# 构建解决方案
dotnet build

# 运行测试
dotnet test
```

### 代码规范

- 遵循 C# 编码规范
- 为公共 API 添加 XML 文档注释
- 为新功能编写单元测试
- 保持代码简洁、易读

## 常见问题

### Q: 如何处理受密码保护的 PDF？

A: 在创建 PdfDocument 时传入密码参数：

```csharp
using var document = new PdfDocument("protected.pdf", "password");
```

### Q: 如何获取页面尺寸？

A: 使用 `PdfPage.Size` 属性：

```csharp
var size = page.Size;  // 返回 SKSizeI（单位：像素）
```

### Q: 如何设置渲染质量？

A: 通过 DPI 控制：

```csharp
var settings = new ImageGenerationSettings { Dpi = 300 };
```

### Q: 是否支持修改 PDF？

A: 当前版本仅支持读取和渲染，修改功能（如合并、拆分）计划在未来版本中实现。

## 许可证

本项目采用 [Apache License 2.0](LICENSE) 许可证。

## 致谢

- [PDFium](https://pdfium.googlesource.com/pdfium/) - Google 的 PDF 渲染引擎
- [bblanchon/PDFium](https://github.com/bblanchon/Arduino-PDFium) - PDFium 预编译二进制包

## 联系方式

- GitHub Issues: https://github.com/Dtronix/PDFiumZ/issues
- 作者: DJGosnell

---

**注意**：本项目当前处于 Beta 阶段，API 可能会在后续版本中变化。建议在生产环境使用前进行充分测试。
