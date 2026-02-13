# PDFium.Z 多目标框架支持

## 当前状态

### ✅ 支持的目标框架

| 框架 | PDFiumZ | PDFiumZ.HighLevel | PDFiumZDemo.SkiaSharp | 状态 |
|--------|----------|-------------------|----------------------|------|
| net8.0 | ✅ | ✅ | ✅ | 完全支持 |
| net9.0 | ✅ | ✅ | ✅ | 完全支持 |
| net10.0 | ⚠️ | ⚠️ | ⚠️ | 待验证（需要 SDK 预览版）|

### ❌ 不支持的目标框架

| 框架 | 原因 | 解决方案 |
|--------|--------|----------|
| netstandard2.0 | SkiaSharp 2.88.8 不支持 | 使用 SkiaSharp 3.x 或创建适配层 |
| netstandard2.1 | SkiaSharp 2.88.8 不支持 | 使用 SkiaSharp 3.x 或创建适配层 |

## 技术细节

### 图形库

**PDFiumZ.HighLevel** 使用 **SkiaSharp 2.88.8** 进行跨平台图像处理：

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net9.0'">
  <PackageReference Include="SkiaSharp" Version="2.88.8" />
</ItemGroup>
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <PackageReference Include="SkiaSharp" Version="2.88.8" />
</ItemGroup>
```

### Polyfills

为 **netstandard2.0/netstandard2.1** 创建了 nint/nuint 类型的 polyfill：

```
System/Polyfills/NintPolyfills.cs
```

当这些框架被支持时，polyfill 将自动启用。

### PDFium 绑定库

**PDFiumZ** 项目支持：
- `net8.0` (.NET 8.0)
- `net9.0` (.NET 9.0)

使用多目标框架：
```xml
<TargetFrameworks>net8.0;net9.0</TargetFrameworks>
```

## 编译结果

### ✅ 成功

```bash
# net9.0
dotnet build PDFiumZ.HighLevel/PDFiumZ.HighLevel.csproj -f net9.0
# 结果: 已成功生成，0 个错误
```

### ⚠️ 问题

```bash
# net8.0 (当构建整个解决方案时)
dotnet build PDFiumZ.sln
# 问题: PDFiumZ 项目的 CppSharp 在 net8.0 下有兼容性问题
# 错误数: 46 个错误（来自 CppSharp 包）
```

**解决方案**: 单独构建各个目标框架，或使用 `dotnet build -f <framework>` 选择特定框架。

## 使用指南

### 编译特定框架

```bash
# 编译 net9.0 版本
dotnet build PDFiumZ.HighLevel/PDFiumZ.HighLevel.csproj -f net9.0

# 编译 net8.0 版本
dotnet build PDFiumZ.HighLevel/PDFiumZ.HighLevel.csproj -f net8.0

# 编译所有支持的框架
dotnet build PDFiumZ.HighLevel/PDFiumZ.HighLevel.csproj
```

### 运行 Demo

```bash
# 使用 net9.0 运行
cd PDFiumZDemo.SkiaSharp
dotnet run -f net9.0

# 使用 net8.0 运行
cd PDFiumZDemo.SkiaSharp
dotnet run -f net8.0
```

## API 设计

### 核心类

- **PdfDocument** - PDF 文档操作（QuestPDF 风格 API）
- **PdfPage** - PDF 页面操作
- **PdfBitmap** - 图像渲染（使用 SkiaSharp）
- **PdfPageCollection** - 页面集合管理
- **ImageGenerationSettings** - 图像生成配置

### 图像生成 API

```csharp
// 返回字节数组枚举
IEnumerable<byte[]> GenerateImages()
IEnumerable<byte[]> GenerateImages(ImageGenerationSettings settings)

// 保存到文件
void GenerateImages(Func<int, string> fileNameCallback)
void GenerateImages(Func<int, string> fileNameCallback, ImageGenerationSettings settings)

// 保存到目录
void GenerateImagesToDirectory(string directory, string baseName = "page", ImageGenerationSettings? settings = null)
```

## 后续改进

### 短期 (1-2 周)

1. **net10.0 支持**
   - 验证 SkiaSharp 2.88.8 与 .NET 10.0 的兼容性
   - 更新项目配置添加 net10.0 目标
   - 测试所有功能

2. **netstandard 支持** (可选)
   - 研究使用 SkiaSharp 3.x（支持 netstandard）
   - 或创建独立的适配层
   - 评估是否值得增加复杂度

3. **文档和示例**
   - 更新 README 说明多框架支持
   - 创建各框架的特定示例
   - 添加性能基准测试

### 中期 (1-3 月)

1. **跨平台验证**
   - Linux 测试
   - macOS 测试
   - ARM64 平台测试

2. **性能优化**
   - 异步 API
   - 流式处理
   - 内存优化

3. **单元测试**
   - 添加 xUnit 测试项目
   - 覆盖率目标 >80%

## 相关文档

- [HIGH_LEVEL_API_COMPLETE.md](HIGH_LEVEL_API_COMPLETE.md) - 高层 API 实现文档
- [API_DESIGN_IMPROVED.md](API_DESIGN_IMPROVED.md) - QuestPDF 风格 API 设计
- [FEATURE_ROADMAP.md](FEATURE_ROADMAP.md) - 功能路线图
