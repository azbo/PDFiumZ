# PDFium.Z 功能规划与下一步建议

## 项目概述

**PDFium.Z** 是一个基于 Google PDFium 的 .NET PDF 处理库，通过 CppSharp 自动生成 P/Invoke 绑定，提供跨平台（Windows、Linux、macOS）的 PDF 渲染和处理能力。

---

## 一、项目现状分析

### 1.1 已完成功能

| 模块 | 功能 | 状态 | 位置 |
|------|------|------|------|
| **底层绑定** | P/Invoke 自动生成（16000+行代码） | ✅ 完成 | `PDFiumZ.cs` |
| **平台支持** | Win-x64/x86, Linux-x64, OSX-x64 | ✅ 完成 | `runtimes/` |
| **高层 API** | `PdfDocument`, `PdfPage`, `PdfBitmap` | ✅ 完成 | `PDFiumZ.HighLevel/` |
| **图像生成** | QuestPDF 风格 API | ✅ 完成 | `PdfDocument.GenerateImages()` |
| **渲染功能** | 位图渲染、多格式导出 | ✅ 完成 | `PdfPage.Render()` |
| **元数据读取** | PDF 元信息提取 | ✅ 完成 | `PdfMetadata` |
| **表单基础** | 表单字段类型识别 | ✅ 部分 | `PdfFormField` |
| **链接提取** | PDF 链接检测 | ✅ 部分 | `PdfLink` |
| **SkiaSharp** | 跨平台图形支持 | ✅ 完成 | 替换 System.Drawing |
| **多框架** | .NET 8.0/9.0 支持 | ✅ 完成 | `net8.0;net9.0` |
| **单元测试** | 基础测试用例 | ✅ 部分 | `PDFiumZ.Tests/` |

### 1.2 现有项目结构

```
src/
├── PDFiumZ/                      # 底层 P/Invoke 绑定（自动生成）
│   └── PDFiumZ.cs               # 16412 行代码
├── PDFiumZ.HighLevel/            # 高层 API 封装
│   ├── PdfDocument.cs           # 文档类
│   ├── PdfPage.cs               # 页面类
│   ├── PdfBitmap.cs             # 位图类（SkiaSharp）
│   ├── PdfPageCollection.cs     # 页面集合
│   ├── PdfDocumentOperations.cs # 文档操作
│   └── Types/                   # 枚举和类型定义
├── PDFiumZ.Tests/                # 单元测试
├── PDFiumZDemo.SkiaSharp/       # 示例程序
└── PDFiumZBindingsGenerator/    # 绑定生成器
```

### 1.3 技术栈

- **核心库**: CppSharp (自动绑定生成)
- **图形处理**: SkiaSharp 3.119.2
- **目标框架**: .NET 8.0, .NET 9.0
- **底层 PDF**: pdfium-binaries (bblanchon)

---

## 二、PDFium API 能力分析

基于对 `PDFiumZ.cs` 代码的分析，项目已绑定的 PDFium 功能包括：

### 2.1 已绑定的核心功能模块

| 模块 | 功能 | 底层函数前缀 |
|------|------|-------------|
| 文档加载 | 文件/内存加载 | `FPDF_LoadDocument`, `FPDF_LoadMemDocument` |
| 页面操作 | 获取尺寸、加载页面 | `FPDF_GetPageCount`, `FPDF_LoadPage` |
| 渲染 | 位图渲染、矩阵变换 | `FPDF_RenderPageBitmapWithMatrix` |
| 文本 | 文本对象操作 | `FPDFText_*` |
| 表单 | 表单交互 | `FORM_*` |
| 注解 | 注解操作 | `FPDFAnnot_*` |
| 页面对象 | 图形/文本对象 | `FPDFPageObj_*` |
| 字体 | 字体加载 | `FPDFText_LoadFont` |

### 2.2 可用但未封装的 API（高层 API 缺失）

| 功能 | PDFium 函数 | 建议优先级 |
|------|-------------|-----------|
| **文本提取** | `FPDFText_*` | 🔴 高 |
| **图像提取** | `FPDF_*Image` | 🔴 高 |
| **页面操作** | `FPDFPage_*` | 🟡 中 |
| **注解/书签** | `FPDF_*Annot*`, `FPDF_*Outline*` | 🟡 中 |
| **文档编辑** | `FPDF_*Document*` | 🟢 低 |
| **安全/加密** | `FPDF_*Crypt*` | 🟢 低 |

---

## 三、下一步功能规划建议

### 3.1 第一阶段：核心功能补全（高优先级）

#### 3.1.1 文本提取功能 🔴 **最高优先级**

这是 PDF 处理最常用的功能，也是 README TODO 中明确提到的需求。

**实现目标**：
```csharp
// 目标 API 设计
public class PdfTextExtractor
{
    // 纯文本提取
    string ExtractText(PdfPage page);
    
    // 带位置的文本片段（用于搜索高亮）
    IEnumerable<TextFragment> ExtractTextWithPositions(PdfPage page);
    
    // 文本搜索
    IEnumerable<TextSearchResult> Search(string query);
}

// 使用示例
using var document = new PdfDocument("sample.pdf");
using var page = document[0];
string text = page.ExtractText();  // "Hello World"
```

**底层 API 分析**（基于 PDFiumZ.cs）：
- `fpdftext.FPDFText_LoadTextPage()` - 加载文本页
- `fpdftext.FPDFText_GetText()` - 获取文本内容
- `fpdftext.FPDFText_CountChars()` - 字符计数
- `fpdftext.FPDFText_GetCharBox()` - 字符边界框

**依赖**：无需新增绑定，已在 `PDFiumZ.cs` 中存在

#### 3.1.2 图像提取功能 🔴 **高优先级**

**实现目标**：
```csharp
// 目标 API 设计
public class PdfImageExtractor
{
    // 获取页面所有图像
    IEnumerable<PdfImageInfo> GetImages(PdfPage page);
    
    // 提取图像数据
    byte[] ExtractImage(PdfImageInfo image);
    
    // 转换为 .NET 图像对象
    SKBitmap ToSkBitmap(PdfImageInfo image);
}
```

**底层 API 分析**：
- `fpdfview.FPDFPage_CountObjects()` - 对象计数
- `fpdfview.FPDFPage_GetObject()` - 获取对象
- `fpdfview.FPDFImageObj_GetImage()` - 获取图像

### 3.2 第二阶段：功能扩展（中优先级）

#### 3.2.1 注解（Annotation）支持

**实现目标**：
```csharp
// 目标 API 设计
public class PdfAnnotation
{
    public PdfAnnotationType Type { get; }
    public PdfRectangle Bounds { get; }
    public string Contents { get; set; }
    public PdfLink? AsLink() => this as PdfLink;
}

public class PdfAnnotationCollection
{
    public IEnumerable<PdfAnnotation> Annotations { get; }
    public void Add(PdfAnnotation annotation);
    public void Remove(PdfAnnotation annotation);
}
```

#### 3.2.2 书签/大纲（Outline）支持

**实现目标**：
```csharp
public class PdfOutline
{
    public string Title { get; }
    public int PageIndex { get; }
    public PdfOutline? Parent { get; }
    public IEnumerable<PdfOutline> Children { get; }
}

public class PdfOutlineCollection
{
    public IEnumerable<PdfOutline> RootItems { get; }
}
```

#### 3.2.3 页面操作

**实现目标**：
```csharp
public class PdfDocument
{
    // 删除页面
    public void RemovePage(int index);
    
    // 插入空白页
    public void InsertPage(int index, float width, float height);
    
    // 旋转页面
    public void RotatePage(int index, PdfRotation rotation);
    
    // 提取页面到新文档
    public PdfDocument ExtractPages(IEnumerable<int> indices);
    
    // 合并文档
    public void Append(PdfDocument otherDocument);
}
```

### 3.3 第三阶段：高级功能（低优先级）

#### 3.3.1 表单交互增强

当前已有基础类型 `PdfFormField`，可扩展：
```csharp
public class PdfFormField
{
    public string Name { get; }
    public PdfFormFieldType FieldType { get; }
    
    // 文本框
    public string TextValue { get; set; }
    
    // 复选框
    public bool IsChecked { get; set; }
    
    // 下拉框
    public int SelectedIndex { get; set; }
    
    // 保存表单
    public void Save();
}
```

#### 3.3.2 文档创建

```csharp
public class PdfDocumentBuilder
{
    public static PdfDocumentBuilder Create();
    
    public PdfDocumentBuilder AddPage(float width, float height);
    public PdfDocumentBuilder DrawText(string text, float x, float y);
    public PdfDocumentBuilder DrawImage(byte[] imageData, float x, float y, float width, float height);
    public PdfDocumentBuilder Save(string path);
}
```

#### 3.3.3 安全功能

```csharp
public class PdfSecurity
{
    public bool IsEncrypted { get; }
    public PdfPermissions Permissions { get; }
    
    public bool VerifyPassword(string password);
    public void SetPassword(string password);
}
```

---

## 四、技术债务与改进

### 4.1 文档完善

**现状**：README TODO 明确提到需要完善文档

| 任务 | 优先级 | 难度 |
|------|--------|------|
| XML 文档注释生成 | 🔴 高 | 中 |
| API 使用示例 | 🔴 高 | 低 |
| 架构设计文档 | 🟡 中 | 中 |
| 迁移指南 | 🟢 低 | 低 |

### 4.2 代码质量

| 任务 | 目标 | 现状 |
|------|------|------|
| 单元测试覆盖 | >80% | ~30% |
| 性能基准测试 | B/S 对比 | 无 |
| 集成测试 | CI 覆盖 | 部分 |

### 4.3 平台扩展

| 平台 | 状态 | 说明 |
|------|------|------|
| Windows x64/x86 | ✅ 完成 | |
| Linux x64 | ✅ 完成 | |
| macOS x64 | ✅ 完成 | |
| **ARM64** | ❌ 缺失 | README TODO 提到 |
| **Windows ARM64** | ❌ 缺失 | 移动设备支持 |

---

## 五、推荐实施路线图

### 阶段一：文本提取（1-2周）

```
Week 1-2: 文本提取功能
├── 创建 PdfTextExtractor 类
├── 实现 FPDFText_* 绑定封装
├── 文本位置信息提取
├── 文本搜索功能
└── 单元测试
```

### 阶段二：图像提取（1-2周）

```
Week 3-4: 图像提取功能
├── 创建 PdfImageExtractor 类
├── 实现 FPDFImageObj_* 绑定封装
├── 图像格式转换
└── 单元测试
```

### 阶段三：注解和书签（2-3周）

```
Week 5-7: 注解和书签
├── PdfAnnotation 类型完善
├── PdfOutline 树结构实现
├── 书签导航功能
└── 集成测试
```

### 阶段四：页面操作（2-3周）

```
Week 8-10: 页面操作
├── 页面删除/插入/旋转
├── 文档合并
├── 页面提取
└── 性能优化
```

### 阶段五：表单和安全（2-3周）

```
Week 11-13: 表单和安全
├── 表单字段读写
├── 密码保护
├── 权限管理
└── 完整测试
```

---

## 六、参考资源

### 类似项目对比

| 库 | 语言 | 特点 | 可借鉴 |
|----|------|------|--------|
| [PdfiumViewer](https://github.com/pvginkel/PdfiumViewer) | C# | Windows Forms | 高层 API 设计 |
| [PDFiumSharp](https://github.com/ArgusMagnus/PDFiumSharp) | C# | 跨平台 | 资源管理 |
| [Patagames Pdfium.Net](https://patagames.com/) | C# | 商业 | 完整 API |

### 官方资源

- [PDFium Source](https://pdfium.googlesource.com/pdfium/)
- [pdfium-binaries](https://github.com/bblanchon/pdfium-binaries)
- [PDFium C API 文档](https://pdfium.googlesource.com/pdfium/+/refs/heads/main/public/)

---

## 七、结论与建议

### 立即行动（1-2周）

1. **实现文本提取功能** - 最高价值，用户最需要
2. **完善单元测试覆盖率** - 为后续功能提供保障
3. **添加 XML 文档注释** - 提升库的可发现性

### 短期目标（1-2月）

1. 图像提取功能
2. 注解支持
3. 书签/大纲支持

### 中期目标（3-6月）

1. 页面操作（删除、插入、旋转）
2. 表单交互增强
3. 文档合并/拆分

### 长期目标（6-12月）

1. ARM64 平台支持
2. 文档创建功能
3. 安全/加密支持

---

**总结**：PDFium.Z 项目拥有扎实的底层绑定基础和良好的高层 API 设计。下一步应优先实现**文本提取功能**，这是用户最常用的功能，也是提升库价值的关键。同时建议逐步完善单元测试和文档，为后续功能扩展奠定基础。
