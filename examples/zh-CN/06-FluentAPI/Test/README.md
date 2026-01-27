# PDFiumZ 中文字体支持说明

## 概述

PDFiumZ 现已完整支持中文字体！通过使用 `FPDFPageObjCreateTextObj` API 替代 `FPDFPageObjNewTextObj`，我们可以直接使用字体句柄来创建文本对象，从而支持自定义 TrueType/OpenType 字体。

## 修改的文件

### 1. `PdfFont.cs` - 添加字体类型检测

```csharp
// 添加属性
public bool IsCustomFont { get; }

// 更新构造函数
internal PdfFont(FpdfFontT handle, PdfDocument document, string name, bool isCustomFont = false)

// 更新自定义字体加载
return new PdfFont(handle, document, "CustomFont", isCustomFont: true);
```

### 2. `PdfContentEditor.cs` - 支持自定义字体

```csharp
// 根据字体类型选择不同的 API
if (font.IsCustomFont)
{
    // 使用字体句柄（支持自定义字体）
    textObj = fpdf_edit.FPDFPageObjCreateTextObj(
        _page._document._handle!,
        font.Handle,
        (float)fontSize);
}
else
{
    // 使用字体名称（仅支持标准字体）
    textObj = fpdf_edit.FPDFPageObjNewTextObj(
        _page._document._handle!,
        font.Name,
        (float)fontSize);
}
```

### 3. `FluentDocument.cs` - 添加 FontHelper 辅助类

```csharp
public static class FontHelper
{
    // 自动加载中文字体
    public static PdfFont LoadChineseFont(PdfDocument document)

    // 从路径加载自定义字体
    public static PdfFont LoadCustomFont(PdfDocument document, string fontPath)

    // 加载中文字体，失败则回退到标准字体
    public static PdfFont LoadChineseFontOrDefault(PdfDocument document, PdfStandardFont defaultFont = PdfStandardFont.Helvetica)
}
```

## 使用方法

### 方法 1: 直接加载字体文件

```csharp
using var doc = PdfDocument.CreateNew();
using var page = doc.CreatePage(595, 842);

// 加载中文字体
using var font = PdfFont.Load(doc, @"C:\Windows\Fonts\simhei.ttf", isCidFont: true);

using var editor = page.BeginEdit();
editor
    .WithFont(font)
    .WithFontSize(16)
    .Text("欢迎使用 PDFiumZ！", 100, 750)
    .Commit();

doc.Save("output.pdf");
```

### 方法 2: 使用 FontHelper（推荐）

```csharp
using var doc = PdfDocument.CreateNew();
using var font = FontHelper.LoadChineseFont(doc); // 自动检测系统字体

// 使用字体...
```

### 方法 3: 带回退的字体加载

```csharp
// 尝试加载中文字体，失败则使用标准字体
using var font = FontHelper.LoadChineseFontOrDefault(doc);
```

## 支持的字体格式

- ✅ TTF (TrueType Font) - 推荐
- ✅ OTF (OpenType Font)
- ⚠️ TTC (TrueType Collection) - 部分支持（某些 TTC 文件可能加载失败）

## 系统字体路径

### Windows
```csharp
@"C:\Windows\Fonts\simhei.ttf"    // 黑体
@"C:\Windows\Fonts\simsun.ttf"    // 宋体
@"C:\Windows\Fonts\msyh.ttf"      // 微软雅黑
```

### macOS
```csharp
@"/System/Library/Fonts/PingFang.ttc"  // 苹方
```

### Linux
```csharp
@"/usr/share/fonts/truetype/droid/DroidSansFallbackFull.ttf"
```

## 测试示例

运行测试程序：

```bash
cd C:\work\net\PDFiumZ\examples\zh-CN\06-FluentAPI\Test
dotnet run
```

生成的文件：
- `example-basic-chinese.pdf` - 基础中文示例
- `example-multi-language.pdf` - 多语言混合
- `example-with-helper.pdf` - 使用 FontHelper

## 技术细节

### 为什么需要修改？

PDFium 的 `FPDFPageObjNewTextObj` 函数只能使用 14 种标准 PDF 字体：
- Helvetica, Helvetica-Bold, Helvetica-Oblique, Helvetica-BoldOblique
- Times-Roman, Times-Bold, Times-Italic, Times-BoldItalic
- Courier, Courier-Bold, Courier-Oblique, Courier-BoldOblique
- Symbol, ZapfDingbats

这些标准字体不支持中文字符。

### 解决方案

使用 `FPDFPageObjCreateTextObj` 函数，它接受字体句柄而不是字体名称，可以支持任何已加载的字体。

### 性能考虑

- 标准字体：不嵌入到 PDF，文件小（~1KB）
- 自定义字体：完整嵌入，文件大（~4-5MB）

## 限制

1. **TTC 字体**：TrueType Collection 文件包含多个字体，PDFium 可能无法正确处理。建议使用单独的 TTF 文件。

2. **字体许可**：请确保您有权在 PDF 中嵌入和分发使用的字体。

3. **跨平台**：字体路径因操作系统而异，使用 `FontHelper.LoadChineseFont()` 可以自动检测。

## 兼容性

- ✅ PDF 1.7
- ✅ Adobe Acrobat Reader
- ✅ 浏览器内置 PDF 查看器
- ✅ 跨平台（Windows, macOS, Linux）

## 反馈

如有问题，请：
1. 检查字体文件是否存在
2. 尝试使用 TTF 而不是 TTC
3. 查看测试程序的输出

---

**最后更新**: 2026-01-23
**版本**: PDFiumZ 146.0.7643.0
