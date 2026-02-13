# PDFium.Z 多目标框架支持 - 更新

## ✅ 当前支持状态

### PDFiumZ.HighLevel 支持

| 框架 | SkiaSharp 版本 | 编译状态 | 运行状态 |
|--------|---------------|----------|----------|
| net9.0 | SkiaSharp 3.119.2 | ✅ 0 个错误 | ✅ Demo 成功运行 |
| net8.0 | SkiaSharp 3.119.2 | ⚠️ 46 个错误 (PDFiumZ CppSharp 问题) | N/A |
| netstandard2.0 | SkiaSharp 3.119.2 | ❌ CppSharp 不支持 | N/A |
| netstandard2.1 | SkiaSharp 3.119.2 | ❌ CppSharp 不支持 | N/A |

### 说明

**net9.0**: 完全支持 ✅
- 编译成功（0 个错误）
- Demo 成功运行
- 所有 6 个示例正常工作
- 图像生成正常（~445 KB）

**net8.0**: 部分 ⚠️
- PDFiumZ.HighLevel 编译失败（46 个 CppSharp 错误）
- 原因：PDFiumZ 项目在 net8.0 下 CppSharp 有兼容性问题
- 解决方案：单独使用 `dotnet build -f net8.0`，或使用 artifacts 中的预编译库

**netstandard2.0/2.1**: 不支持 ❌
- SkiaSharp 3.119.2 支持 netstandard，但 CppSharp（PDFiumZ 依赖）不支持
- CppSharp 1.1.84 在 .NET Standard 下不可用

## 项目配置

### PDFiumZ.csproj
```xml
<TargetFrameworks>net8.0;net9.0</TargetFrameworks>
```

### PDFiumZ.HighLevel.csproj
```xml
<TargetFrameworks>net8.0;net9.0</TargetFrameworks>
<PackageReference Include="SkiaSharp" Version="3.119.2" />
```

### PDFiumZDemo.SkiaSharp.csproj
```xml
<TargetFrameworks>net8.0;net9.0</TargetFrameworks>
<ProjectReference Include="..\PDFiumZ.HighLevel\PDFiumZ.HighLevel.csproj" />
```

## 使用指南

### 编译特定框架

```bash
# ✅ 推荐：使用 net9.0（完全支持）
dotnet build PDFiumZ.HighLevel/PDFiumZ.HighLevel.csproj -f net9.0

# ⚠️ net8.0 单独编译
dotnet build PDFiumZ/PDFiumZ.csproj -f net8.0
dotnet build PDFiumZ.HighLevel/PDFiumZ.HighLevel.csproj -f net8.0
```

### 运行 Demo

```bash
# ✅ 推荐：使用 net9.0
cd PDFiumZDemo.SkiaSharp
dotnet run -f net9.0

# 所有 6 个示例成功运行
# - 文档信息读取
# - 图像生成（PNG 格式，~445 KB）
# - 目录批量保存
```

## 技术栈

### 图形库
- **SkiaSharp 3.119.2** - 最新稳定版本
- 支持 netstandard2.0/2.1
- 跨平台图像处理

### PDFium 绑定
- **CppSharp 1.1.84.17100** - 代码生成
- 支持 net8.0/net9.0（.NET 平台）
- **不支持** netstandard2.0/2.1

## 关于 netstandard 支持的说明

虽然 SkiaSharp 3.119.2 官方支持 netstandard2.0/2.1，但由于 PDFiumZ 项目的依赖 CppSharp 在这些框架下不可用，导致无法编译。

### 可行的解决方案

1. **使用 artifacts 中的预编译库**
   ```xml
   <ItemGroup>
     <Reference Include="..\..\artifacts\netstandard2.0\PDFiumZ.dll" />
   </ItemGroup>
   ```

2. **创建独立的 netstandard 包**
   - 将 PDFiumZ.HighLevel 打包为纯 netstandard2.0/2.1 包
   - 依赖预编译的 PDFiumZ 原生库
   - 分离 CppSharp 依赖

3. **条件编译排除**
   ```csharp
   #if !NETSTANDARD2_0
       // 使用 CppSharp 生成的代码
   #else
       // 提供预编译接口
   #endif
   ```

## 示例输出

```
=== PDFium.Z 高层 API 示例 ===

--- 示例 1: 基础用法 ---
文档页数: 1
  页面 0: 595.00 x 842.00 点

--- 示例 2: 单页图像 ---
已保存: output-single.png (595.00 x 842.00)

--- 示例 3: 所有页面为字节数组 ---
  页面 0: 445,330 字节

--- 示例 4: 保存所有页面 ---
已保存: page0.png, page1.png, ...
已保存: document_page_000.png, document_page_001.png, ...

--- 示例 5: 自定义设置 ---
使用 DPI 300 生成了 1 个图像

--- 示例 6: 保存到目录 ---
已保存所有页面到: C:\work\net\PDFium.Z\src\PDFiumZDemo.SkiaSharp\output
```

## 相关文档

- [HIGH_LEVEL_API_COMPLETE.md](HIGH_LEVEL_API_COMPLETE.md) - 高层 API 完成文档
- [API_DESIGN_IMPROVED.md](API_DESIGN_IMPROVED.md) - QuestPDF 风格 API 设计
- [FEATURE_ROADMAP.md](FEATURE_ROADMAP.md) - 功能路线图
