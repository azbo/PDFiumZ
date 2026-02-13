# PDFium.Z 开发快速开始指南

## 文档索引

- [功能规划文档](FEATURE_ROADMAP.md) - 完整的功能路线图和优先级
- [API 设计文档](API_DESIGN.md) - 高层 API 详细设计建议

---

## 立即可以开始的任务

### 任务 1：创建高层 API 基础框架

**位置**：`src/PDFiumZ.HighLevel/`

**步骤**：

1. 创建新的类库项目
```bash
cd src
dotnet new classlib -n PDFiumZ.HighLevel
dotnet add PDFiumZ.HighLevel/PDFiumZ.HighLevel.csproj reference ../PDFiumZ/PDFiumZ.csproj
```

2. 创建目录结构
```
PDFiumZ.HighLevel/
├── PdfDocument.cs          # 文档类
├── PdfPage.cs              # 页面类
├── PdfBitmap.cs            # 位图类
├── PdfRenderOptions.cs     # 渲染选项
├── PdfPageCollection.cs    # 页面集合
└── Types/
    ├── PdfEnums.cs         # 枚举类型
    ├── PdfRectangle.cs     # 矩形结构
    └── ...
```

3. 更新解决方案
```bash
dotnet sln PDFiumZ.sln add src/PDFiumZ.HighLevel/PDFiumZ.HighLevel.csproj
```

### 任务 2：实现 PdfDocument 类基础功能

**目标**：提供基本的文档加载和页面访问功能

**最小可行实现**：
```csharp
namespace PDFiumZ.HighLevel;

public class PdfDocument : IDisposable
{
    private FpdfDocumentT _handle;

    public PdfDocument(string filePath, string? password = null)
    {
        fpdfview.FPDF_InitLibrary();
        _handle = fpdfview.FPDF_LoadDocument(filePath, password);
        if (_handle == null || _handle.IsInvalid)
            throw new InvalidOperationException("Failed to load PDF document");
    }

    public int PageCount => (int)fpdfview.FPDF_GetPageCount(_handle);

    public PdfPage this[int index] => new PdfPage(this, index);

    public void Dispose()
    {
        _handle?.Dispose();
        fpdfview.FPDF_DestroyLibrary();
    }
}
```

### 任务 3：实现 PdfPage 渲染功能

**目标**：简化页面到位图的渲染操作

**最小可行实现**：
```csharp
public class PdfPage : IDisposable
{
    private readonly PdfDocument _document;
    private readonly int _index;
    private FpdfPageT? _handle;

    public float Width { get; private set; }
    public float Height { get; private set; }

    public PdfBitmap Render(PdfRenderOptions? options = null)
    {
        options ??= new PdfRenderOptions();

        var scale = options.Scale;
        var width = (int)(Width * scale);
        var height = (int)(Height * scale);

        var bitmap = fpdfview.FPDFBitmapCreateEx(width, height, (int)FPDFBitmapFormat.BGRA, IntPtr.Zero, 0);

        if (options.BackgroundColor.HasValue)
        {
            var color = (uint)options.BackgroundColor.Value.ToArgb();
            fpdfview.FPDFBitmapFillRect(bitmap, 0, 0, width, height, color);
        }

        using var matrix = new FS_MATRIX_();
        using var clipping = new FS_RECTF_();

        matrix.A = scale;
        matrix.D = scale;

        fpdfview.FPDF_RenderPageBitmapWithMatrix(
            bitmap, _handle, matrix, clipping,
            (int)options.Flags);

        return new PdfBitmap(bitmap, width, height);
    }
}
```

---

## 单元测试项目

### 创建测试项目

```bash
cd src
dotnet new xunit -n PDFiumZ.HighLevel.Tests
dotnet add PDFiumZ.HighLevel.Tests/PDFiumZ.HighLevel.Tests.csproj reference ../PDFiumZ.HighLevel/PDFiumZ.HighLevel.csproj
```

### 示例测试

```csharp
public class PdfDocumentTests
{
    [Fact]
    public void LoadDocument_Succeeds()
    {
        using var pdf = new PdfDocument("test.pdf");
        Assert.Equal(1, pdf.PageCount);
    }

    [Fact]
    public void RenderPage_CreatesBitmap()
    {
        using var pdf = new PdfDocument("test.pdf");
        using var page = pdf[0];
        using var bitmap = page.Render();

        Assert.True(bitmap.Width > 0);
        Assert.True(bitmap.Height > 0);
    }
}
```

---

## Demo 项目更新

### 更新现有 Demo

编辑 `src/PDFiumZDemo/Program.cs`：

```csharp
using PDFiumZ.HighLevel;

class Program
{
    static void Main(string[] args)
    {
        // 高层 API 用法
        using var pdf = new PdfDocument("pdf-sample.pdf");
        Console.WriteLine($"Pages: {pdf.PageCount}");

        using var page = pdf[0];
        Console.WriteLine($"Size: {page.Width} x {page.Height}");

        using var bitmap = page.Render(new PdfRenderOptions
        {
            Scale = 2.0f,
            BackgroundColor = System.Drawing.Color.White
        });

        Console.WriteLine($"Rendered: {bitmap.Width}x{bitmap.Height}");
        bitmap.SaveAsPng("output.png");
        Console.WriteLine("Saved to output.png");
    }
}
```

---

## 推荐的实施顺序

### 第 1 天：项目结构搭建
- [ ] 创建 `PDFiumZ.HighLevel` 项目
- [ ] 创建 `PDFiumZ.HighLevel.Tests` 项目
- [ ] 设置项目引用

### 第 2-3 天：核心类实现
- [ ] 实现 `PdfDocument` 基础功能（加载、页面数、Dispose）
- [ ] 实现 `PdfPage` 基础功能（尺寸、索引）
- [ ] 实现 `PdfPageCollection` 集合类

### 第 4-5 天：渲染功能
- [ ] 实现 `PdfRenderOptions` 类
- [ ] 实现 `PdfBitmap` 类
- [ ] 实现 `PdfPage.Render()` 方法

### 第 6-7 天：测试和示例
- [ ] 编写单元测试
- [ ] 更新 Demo 项目
- [ ] 验证功能正确性

---

## 依赖项建议

### 高层 API 可选依赖

```xml
<PackageReference Include="System.Drawing.Common" Version="*" />
<!-- 用于 Windows 平台图像支持 -->

<PackageReference Include="SkiaSharp.NativeAssets.Linux" Version="*" />
<PackageReference Include="SkiaSharp.NativeAssets.macOS" Version="*" />
<!-- 用于跨平台图像支持 -->
```

---

## 命令行快速参考

```bash
# 构建整个解决方案
dotnet build

# 运行测试
dotnet test

# 运行特定项目
dotnet run --project src/PDFiumZDemo

# 添加 NuGet 包
dotnet add package PackageName

# 清理构建输出
dotnet clean
```

---

## 参考链接

| 资源 | 链接 |
|------|------|
| PDFium 官方源码 | https://pdfium.googlesource.com/pdfium/ |
| PDFium API 文档 | https://pdfium.googlesource.com/pdfium/+/refs/heads/main/public/ |
| pdfium-binaries | https://github.com/bblanchon/pdfium-binaries |
| CppSharp | https://github.com/mono/CppSharp |

---

## 下一步

1. **从任务 1 开始**：创建高层 API 基础框架
2. **参考 API_DESIGN.md**：查看详细的类设计
3. **查看 FEATURE_ROADMAP.md**：了解完整的功能规划
4. **迭代实现**：按照推荐顺序逐步实现功能
