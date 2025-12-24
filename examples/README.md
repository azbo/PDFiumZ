# PDFiumZ 示例代码

本目录包含 PDFiumZ 的完整示例代码，按功能模块分类组织。

## 📑 目录结构

### [01-Basics](./01-Basics/) - 基础示例
PDFiumZ 入门和基础操作
- **GettingStarted**: PDFiumZ 快速入门

### [02-Rendering](./02-Rendering/) - 渲染功能
PDF 页面渲染和图像生成
- **ImageGeneration**: 将 PDF 页面渲染为图像
- **Thumbnails**: 生成页面缩略图

### [03-PageManipulation](./03-PageManipulation/) - 页面操作
PDF 文档的页面操作功能
- **RangeOperations**: 使用 Range 语法进行页面操作
- **MergeSplit**: 合并和拆分 PDF 文档

### [04-AdvancedOptions](./04-AdvancedOptions/) - 高级选项
高级配置选项和流畅 API
- **OptionsConfig**: 使用选项类进行精细控制

---

## 🚀 快速开始

### 运行单个示例

每个示例都是独立的可执行项目，可以直接运行：

```bash
# 进入示例目录
cd examples/01-Basics

# 运行示例
dotnet run

# 或者编译后运行
dotnet build
dotnet run --no-build
```

### 示例项目结构

每个示例目录包含：
```
01-Basics/
├── GettingStarted.csproj    # 项目配置文件
├── GettingStarted.cs         # 示例代码
├── README.md                 # 示例说明
└── bin/                      # 编译输出（自动生成）
```

---

## 📚 示例说明

### 基础示例 (01-Basics)

适合初学者的入门示例，演示：
- 初始化 PDFium 库
- 打开 PDF 文档
- 读取页面信息
- 资源清理

### 渲染示例 (02-Rendering)

演示 PDF 渲染功能：
- 将页面渲染为图像（PNG、JPEG 等）
- 自定义渲染质量和 DPI
- 批量生成页面图像
- 生成预览缩略图

### 页面操作示例 (03-PageManipulation)

演示文档操作功能：
- 使用现代 Range 语法（.NET 8+）
- 提取指定页面
- 旋转页面
- 合并多个文档
- 拆分文档

### 高级选项示例 (04-AdvancedOptions)

演示高级配置选项：
- ImageGenerationOptions：控制页面渲染
- ImageSaveOptions：控制图像保存
- ThumbnailOptions：控制缩略图生成
- 流畅 API 链式调用

---

## 💡 使用建议

### 学习路径

1. **初学者**: 01-Basics → 02-Rendering → 03-PageManipulation
2. **有经验开发者**: 直接查看感兴趣的功能模块
3. **深入学习**: 阅读 04-AdvancedOptions 了解最佳实践

### 示例代码特点

- ✅ **独立可运行**: 每个示例都是完整的项目
- ✅ **详细注释**: 代码中有充分的中文注释
- ✅ **多种场景**: 覆盖常见使用场景
- ✅ **最佳实践**: 展示推荐的 API 使用方式

---

## 🔗 相关文档

- [主文档](../docs/README.md) - 完整的 API 文档
- [快速开始](../docs/Getting_Started/Quick_Start.md) - 快速入门指南
- [API 参考](../docs/API_Reference.md) - API 快速参考

---

## 💬 反馈与贡献

如果发现问题或有改进建议，欢迎：
- 提交 Issue
- 发起 Pull Request
- 在讨论区交流

---

**PDFiumZ** - .NET 的现代化 PDF 处理库
