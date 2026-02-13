# PDFium.Z 功能规划文档

## 项目现状分析

### 已实现功能

| 类别 | 功能 | 状态 |
|------|------|------|
| 核心绑定 | P/Invoke 自动绑定生成 (16412行代码) | ✅ 完成 |
| 平台支持 | Linux-x64, OSX-x64, Win-x64, Win-x86 | ✅ 完成 |
| 文档操作 | 加载文档、获取页数、页面尺寸 | ✅ 完成 |
| 渲染功能 | 页面到位图渲染、多种渲染标志 | ✅ 完成 |
| 变换操作 | 页面变换、裁剪路径、页面边框设置 | ✅ 完成 |
| 构建系统 | 自动绑定生成器、版本管理 | ✅ 完成 |
| 测试框架 | 基础单元测试 | ✅ 部分完成 |

### 当前限制

1. **高级封装缺失**：需要直接操作 unsafe 指针和 P/Invoke 调用
2. **文档不完善**：README TODO 中提到需要生成功能性文档
3. **缺少高层API**：没有友好的 .NET 风格 API
4. **ARM 平台支持缺失**：README TODO 中提到需要调查 ARM 构建
5. **示例代码有限**：仅有一个渲染示例

---

## 功能优先级规划

### 第一阶段：高优先级（易用性增强）

#### 1.1 高层 API 封装

**目标**：提供类型安全、易用的 .NET 风格 API

**功能点**：
- `PdfDocument` 类：IDisposable 自动资源管理
- `PdfPage` 类：页面操作封装
- `PdfRenderOptions` 类：渲染选项配置
- `PdfTextExtractor` 类：文本提取功能

**示例代码**：
```csharp
// 当前用法（底层）
using PDFiumZ;
fpdfview.FPDF_InitLibrary();
var doc = fpdfview.FPDF_LoadDocument("file.pdf", null);
var page = fpdfview.FPDF_LoadPage(doc, 0);
// ... 手动管理资源

// 目标用法（高层）
using var pdf = new PdfDocument("file.pdf");
using var page = pdf.Pages[0];
var bitmap = page.Render(new PdfRenderOptions { Scale = 2.0f });
```

#### 1.2 文本提取功能

**目标**：支持从 PDF 页面提取文本内容和位置信息

**功能点**：
- 纯文本提取
- 带坐标的文本提取（用于搜索/高亮）
- 文本搜索功能

#### 1.3 图像提取功能

**目标**：从 PDF 文档中提取图像资源

**功能点**：
- 枚举页面中的所有图像
- 提取原始图像数据
- 支持常见图像格式输出

---

### 第二阶段：中优先级（功能扩展）

#### 2.1 表单交互

**目标**：支持 PDF 表单（AcroForm）的读取和填写

**功能点**：
- 读取表单字段
- 填写表单值
- 保存已填写表单
- 表单字段类型识别（文本框、复选框、单选按钮等）

#### 2.2 注释/书签功能

**目标**：支持 PDF 注释（Annotation）和书签（Outline/Bookmark）

**功能点**：
- 读取/创建注释
- 读取文档大纲/书签结构
- 导航功能

#### 2.3 页面操作

**目标**：支持 PDF 页面的创建、删除、重组

**功能点**：
- 删除页面
- 旋转页面
- 页面顺序重排
- 合并多个 PDF 文档

#### 2.4 安全和权限

**目标**：支持加密 PDF 和权限管理

**功能点**：
- 检测文档权限
- 解密受保护文档
- 设置文档密码

---

### 第三阶段：低优先级（优化和扩展）

#### 3.1 ARM 平台支持

**目标**：添加 ARM64 架构支持

**功能点**：
- Windows ARM64
- Linux ARM64
- macOS ARM64 (Apple Silicon)

#### 3.2 性能优化

**目标**：提升渲染性能和资源使用效率

**功能点**：
- 渲染缓存
- 异步渲染 API
- 内存池优化

#### 3.3 高级渲染功能

**目标**：支持更复杂的渲染场景

**功能点**：
- 多页面平铺渲染
- 缩略图生成
- 打印优化渲染

#### 3.4 文档生成/编辑

**目标**：从零创建 PDF 文档

**功能点**：
- 创建空白页面
- 绘制文本/图形
- 添加图像
- 保存新文档

---

## 技术债务清理

### 文档完善

**优先级**：高（README TODO 项）

**任务**：
- [ ] 创建 XML 文档注释生成器
- [ ] 为公共 API 生成完整文档
- [ ] 创建 API 使用示例库
- [ ] 添加架构设计文档

### 代码质量

**任务**：
- [ ] 完善单元测试覆盖率（目标：>80%）
- [ ] 添加集成测试
- [ ] 性能基准测试
- [ ] 代码审查和安全审计

### 构建和 CI/CD

**任务**：
- [ ] GitHub Actions 工作流完善
- [ ] 自动化 NuGet 发布
- [ ] 多平台 CI 测试
- [ ] 自动化绑定更新

---

## 参考实现资源

### 类似库对比

| 库名称 | 语言 | 特点 | 可借鉴功能 |
|--------|------|------|-----------|
| [PdfiumViewer](https://github.com/pvginkel/PdfiumViewer) | C# | Windows Forms 支持 | 高层 API 设计 |
| [PDFiumSharp](https://github.com/ArgusMagnus/PDFiumSharp) | C# | 跨平台 | 资源管理模式 |
| [Patagames Pdfium.Net](https://patagames.com/) | C# | 商业 SDK | 文本提取 API |

### 官方文档资源

- [PDFium Google Source](https://pdfium.googlesource.com/pdfium/)
- [PDFium API 文档](https://pdfium.googlesource.com/pdfium/+/refs/heads/main/public/)
- [pdfium-binaries](https://github.com/bblanchon/pdfium-binaries) - 预编译二进制文件

---

## 建议的实施顺序

### 立即开始（1-2周）
1. **高层 API 封装框架** - 创建 `PdfDocument` 和 `PdfPage` 类的基础结构
2. **完善单元测试** - 为现有功能补充测试用例

### 短期目标（1-2月）
3. **文本提取功能** - 实现基础的文本提取 API
4. **图像提取功能** - 支持从页面提取图像
5. **文档生成器** - 为公共 API 生成 XML 文档注释

### 中期目标（3-6月）
6. **表单交互** - 实现 AcroForm 支持功能
7. **注释/书签** - 添加注释和书签读取功能
8. **页面操作** - 支持页面删除、旋转、重排

### 长期目标（6-12月）
9. **ARM 平台支持** - 调查并实现 ARM64 支持
10. **文档生成/编辑** - 从零创建 PDF 的功能
11. **性能优化** - 缓存、异步渲染等优化

---

## 结论

PDFium.Z 目前拥有稳固的底层绑定基础，下一步应聚焦于**高层 API 封装**和**常用功能实现**，使其成为一个对 .NET 开发者友好的 PDF 处理库。

建议首先实现**高层 API 封装**和**文本提取功能**，这两个功能能立即提升库的可用性和价值。
