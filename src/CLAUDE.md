# PDFium.Z 项目指南

## 项目概述

PDFium.Z 是一个 .NET PDF 处理库，通过 P/Invoke 技术绑定 Google 的 PDFium C API。项目目标是为 .NET 开发者提供高性能、可靠的 PDF 处理能力。

### 目标用户

- 需要在 .NET 应用中渲染 PDF 的开发者
- 需要 PDF 图像生成功能的应用
- 需要跨平台 PDF 支持的项目

### 使用场景

- PDF 文档预览和渲染
- PDF 转图像服务
- 文档管理系统
- 自动化报表生成

## 架构说明

### 模块划分

```
src/
├── PDFiumZ/                    # 底层 P/Invoke 绑定
│   ├── PDFiumZ.cs             # 主要绑定代码
│   ├── RenderFlags.cs         # 渲染标志
│   └── FPDFBitmapFormat.cs    # 位图格式枚举
├── PDFiumZ.HighLevel/          # 高级 .NET API 封装
│   ├── PdfDocument.cs         # 文档类
│   ├── PdfPage.cs             # 页面类
│   ├── PdfPageCollection.cs   # 页面集合
│   ├── PdfBitmap.cs           # 位图类
│   └── Types/                 # 类型定义
│       ├── ImageFormat.cs
│       ├── ImageGenerationSettings.cs
│       ├── PdfRenderFlags.cs
│       └── PdfRotation.cs
├── PDFiumZ.Tests/              # 测试项目
├── PDFiumZDemo/                # 基础示例项目
└── PDFiumZDemo.SkiaSharp/      # SkiaSharp 集成示例
```

### 设计原则

#### 1. 简洁的 API 设计
- 高级 API 遵循 .NET 命名规范和设计模式
- 使用 `IDisposable` 确保资源正确释放
- 提供默认参数减少调用复杂度

#### 2. 资源自动管理
```csharp
// 推荐：使用 using 自动释放资源
using var document = new PdfDocument("file.pdf");
using var page = document.Pages[0];
using var bitmap = page.Render();
```

#### 3. 跨平台兼容性
- 支持 .NET 8.0、9.0、10.0 和 .NET Standard 2.0/2.1
- 自动加载对应平台的原生 PDFium 库
- 统一的 API 接口，平台差异透明化

#### 4. 分层设计
- **底层（PDFiumZ）**：直接绑定 PDFium C API
- **高层（PDFiumZ.HighLevel）**：面向对象的 .NET 封装

## 开发指南

### 环境要求

#### 必需
- **.NET SDK 9.0** 或更高版本
- **支持的操作系统**：
  - Windows 10/11（x86/x64）
  - Linux（x64）
  - macOS（x64）

#### 可选
- **Git**：用于版本控制
- **Visual Studio 2022** 或 **Visual Studio Code**：IDE 支持

### 构建项目

```bash
# 克隆仓库
git clone https://github.com/Dtronix/PDFiumZ.git
cd PDFiumZ/src

# 还原依赖
dotnet restore

# 构建整个解决方案
dotnet build PDFiumZ.sln

# 仅构建主项目
dotnet build PDFiumZ/PDFiumZ.csproj
dotnet build PDFiumZ.HighLevel/PDFiumZ.HighLevel.csproj
```

### 运行测试

```bash
# 运行所有测试
dotnet test

# 运行特定测试项目
dotnet test PDFiumZ.Tests/PDFiumZ.Tests.csproj

# 运行特定测试类
dotnet test --filter "FullyQualifiedName~PdfDocumentTests"

# 生成覆盖率报告
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

### 打包 NuGet

```bash
# 打包主库
dotnet pack PDFiumZ/PDFiumZ.csproj -c Release

# 打包高级 API
dotnet pack PDFiumZ.HighLevel/PDFiumZ.HighLevel.csproj -c Release

# 输出位置：../artifacts/
```

## 代码规范

### C# 编码规范

#### 1. 命名约定
```csharp
// 类名：PascalCase
public class PdfDocument { }

// 方法：PascalCase
public void RenderPage() { }

// 属性：PascalCase
public int PageCount { get; }

// 私有字段：_camelCase
private readonly FpdfDocumentT? _handle;

// 参数：camelCase
public void Load(string filePath) { }
```

#### 2. 文件组织
```csharp
// 1. using 语句（按字母排序）
using System;
using PDFiumZ;

// 2. 命名空间
namespace PDFiumZ.HighLevel;

// 3. XML 文档注释
/// <summary>
/// 表示 PDF 文档
/// </summary>
public class PdfDocument { }
```

#### 3. XML 文档注释
```csharp
/// <summary>
/// 从文件路径加载 PDF 文档
/// </summary>
/// <param name="filePath">PDF 文件路径</param>
/// <param name="password">文档密码（如有加密）</param>
/// <exception cref="InvalidOperationException">加载失败时抛出</exception>
public PdfDocument(string filePath, string? password = null)
{
    // 实现
}
```

#### 4. 异常处理
```csharp
// 使用标准异常类型
if (_handle == null)
    throw new InvalidOperationException("Failed to load document");

if (index < 0 || index >= PageCount)
    throw new ArgumentOutOfRangeException(nameof(index));
```

#### 5. 资源管理
```csharp
public class PdfDocument : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // 释放托管资源
            }

            // 释放非托管资源
            _handle?.Dispose();

            _disposed = true;
        }
    }
}
```

### 单元测试规范

```csharp
using NUnit.Framework;

namespace PDFiumZ.Tests;

public class PdfDocumentTests
{
    [SetUp]
    public void Setup()
    {
        // 测试前准备
    }

    [TearDown]
    public void TearDown()
    {
        // 测试后清理
    }

    [Test]
    public void LoadDocument_ValidFilePath_ReturnsDocument()
    {
        // Arrange
        var filePath = "test.pdf";

        // Act
        using var document = new PdfDocument(filePath);

        // Assert
        Assert.That(document, Is.Not.Null);
        Assert.That(document.PageCount, Is.GreaterThan(0));
    }

    [Test]
    public void LoadDocument_InvalidFilePath_ThrowsException()
    {
        // Arrange
        var filePath = "nonexistent.pdf";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            using var document = new PdfDocument(filePath);
        });
    }
}
```

## 测试指南

### 测试结构

```
PDFiumZ.Tests/
├── FpdfviewTests.cs           # 底层 API 测试
├── PdfDocumentTests.cs        # 文档类测试（计划中）
├── PdfPageTests.cs            # 页面类测试（计划中）
├── PdfBitmapTests.cs          # 位图类测试（计划中）
└── pdf-sample.pdf             # 测试数据
```

### 测试框架

- **NUnit 4.x**：主测试框架
- **NUnit3TestAdapter 6.x**：测试适配器
- **coverlet.collector**：代码覆盖率收集

### 添加新测试

#### 步骤
1. 在 `PDFiumZ.Tests/` 创建测试类
2. 继承测试命名规范：`XxxTests.cs`
3. 添加 `[Test]` 特性标记测试方法
4. 使用 NUnit 4.x 语法：`Assert.That(..., Is.EqualTo(...))`

#### 示例
```csharp
[Test]
public void PageCount_ReturnsCorrectNumber()
{
    // Arrange
    using var document = new PdfDocument("sample.pdf");

    // Act
    int count = document.PageCount;

    // Assert
    Assert.That(count, Is.EqualTo(1));
}
```

### 测试数据

- 将测试 PDF 文件放在 `src/pdf-sample.pdf`
- 使用 `<CopyToOutputDirectory>` 确保复制到输出目录

## 功能开发路线图

### 已实现

- [x] PDF 文档加载（文件、流、字节数组）
- [x] 页面遍历和访问
- [x] 页面渲染（自定义 DPI、旋转、背景色）
- [x] 图像生成（PNG、JPEG、BMP）
- [x] 资源自动管理
- [x] 密码保护的 PDF 支持
- [x] NUnit 4.x 测试框架兼容性

### 计划中

#### 高优先级
- [ ] **文档元数据提取**
  - 标题、作者、主题、关键词
  - 创建日期、修改日期
  - 应用程序、PDF 生产器

- [ ] **文本提取**
  - 页面文本内容提取
  - 指定区域文本提取
  - 文本位置和字体信息

- [ ] **页面操作**
  - 页面旋转
  - 获取页面注释
  - 获取页面链接

#### 中优先级
- [ ] **表单操作**
  - 表单字段读取
  - 表单填写
  - 表单扁平化

- [ ] **PDF 合并与拆分**
  - 多文档合并
  - 单文档拆分
  - 页面删除/插入

#### 低优先级
- [ ] **注释操作**
  - 创建注释
  - 修改注释
  - 删除注释

- [ ] **数字签名**
  - 签名验证
  - 添加签名

## 常见问题

### Q: 如何添加新的 PDFium 绑定？

A:
1. 在 `PDFiumZ/` 目录添加绑定声明
2. 使用 `[DllImport]` 特性标记
3. 定义对应的类型和结构体
4. 在 `PDFiumZ.HighLevel/` 创建高层封装

### Q: 如何处理不同平台的差异？

A:
PDFium 原生库通过 NuGet 包自动管理：
- `bblanchon.PDFium.Win32`
- `bblanchon.PDFium.Linux`
- `bblanchon.PDFium.macOS`

无需手动处理平台差异。

### Q: 内存泄漏如何调试？

A:
1. 确保 `IDisposable` 对象使用 `using` 语句
2. 检查是否正确调用 `Dispose()`
3. 使用内存分析工具（如 dotMemory）
4. 运行长时间测试验证资源释放

### Q: 如何贡献代码？

A:
1. Fork 仓库
2. 创建功能分支
3. 编写代码和测试
4. 确保所有测试通过
5. 提交 Pull Request

### Q: API 变更策略是什么？

A:
当前处于 Beta 阶段，API 可能变化。建议：
- 监控 Release Notes
- 使用版本锁定
- 参与 API 设计讨论

## 技术债务

### 已知问题
1. 缺少完整的错误消息本地化
2. 部分边界条件未充分测试
3. 文档缺少高级用法示例

### 改进计划
1. 增加单元测试覆盖率至 70%+
2. 完善错误处理和异常消息
3. 添加性能基准测试
4. 生成 API 文档网站

## 相关资源

### 外部链接
- [PDFium 源代码](https://pdfium.googlesource.com/pdfium/)
- [PDFium 文档](https://pdfium.googlesource.com/pdfium/+/refs/heads/main/docs/)
- [SkiaSharp 文档](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/)

### 内部项目
- PDFiumZBindingsGenerator：绑定代码生成器
- CppSharp：C++ 到 C# 绑定生成工具

---

**最后更新**：2025-02-13
**维护者**：DJGosnell
**许可证**：Apache License 2.0
